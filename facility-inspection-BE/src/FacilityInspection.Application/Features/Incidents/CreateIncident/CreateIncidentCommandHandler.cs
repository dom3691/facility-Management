using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Files;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.Incidents.CreateIncident;

/// <summary>
/// Logs a new incident: validates the facility/location, generates the incident number,
/// stores attachments, writes an audit-log entry, queues inspector/admin notifications,
/// and persists it all in one transaction.
/// </summary>
public class CreateIncidentCommandHandler
    : IRequestHandler<CreateIncidentCommand, IncidentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateIncidentCommandHandler> _logger;

    public CreateIncidentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<CreateIncidentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _identityService = identityService;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<IncidentResponse> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var reportedByUserId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        // Referential checks up-front for clear errors (rather than a raw FK failure).
        var facility = await _unitOfWork.Facilities.GetByIdAsync(request.FacilityId, cancellationToken)
            ?? throw new NotFoundException(nameof(Facility), request.FacilityId);

        var location = await _unitOfWork.Repository<Location>().GetByIdAsync(request.LocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.FacilityId != facility.Id)
        {
            throw new BadRequestException("The location does not belong to the specified facility.");
        }

        var incident = new Incident
        {
            IncidentNumber = await GenerateIncidentNumberAsync(cancellationToken),
            BusinessUnit = request.BusinessUnit,
            SAPId = request.SAPId,
            FacilityId = request.FacilityId,
            LocationId = request.LocationId,
            IncidentDate = request.IncidentDate,
            Description = request.Description,
            Status = IncidentStatus.PendingInspection,
            ReportedByUserId = reportedByUserId,
        };

        await StoreAttachmentsAsync(incident, request.Attachments, cancellationToken);

        await _unitOfWork.Incidents.AddAsync(incident, cancellationToken);
        await WriteAuditLogAsync(incident, cancellationToken);
        await QueueInspectionNotificationsAsync(incident, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentNumber} ({IncidentId}) created by {UserId}",
            incident.IncidentNumber, incident.Id, reportedByUserId);

        // Reload with facility/location/attachments so the response is fully populated.
        var created = await _unitOfWork.Incidents.GetByIdWithDetailsAsync(incident.Id, cancellationToken);
        return created!.ToResponse();
    }

    private async Task<string> GenerateIncidentNumberAsync(CancellationToken cancellationToken)
    {
        var year = _dateTime.UtcNow.Year;
        var prefix = $"INC-{year}-";
        var count = await _unitOfWork.Incidents.CountByNumberPrefixAsync(prefix, cancellationToken);
        return $"{prefix}{count + 1:D6}";
    }

    private async Task StoreAttachmentsAsync(
        Incident incident,
        IReadOnlyList<FileUpload> attachments,
        CancellationToken cancellationToken)
    {
        foreach (var file in attachments)
        {
            _fileStorage.ValidateFile(file.FileName, file.SizeBytes);
        }

        foreach (var file in attachments)
        {
            using var stream = new MemoryStream(file.Content);
            var storagePath = await _fileStorage.SaveAsync(
                stream, file.FileName, file.ContentType, FileModules.Incidents, cancellationToken);

            incident.Attachments.Add(new IncidentAttachment
            {
                FileName = file.FileName,
                StoragePath = storagePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.SizeBytes,
            });
        }
    }

    private Task WriteAuditLogAsync(Incident incident, CancellationToken cancellationToken)
        => _auditService.LogWorkflowActionAsync(
            nameof(Incident),
            incident.Id.ToString(),
            AuditActions.IncidentCreated,
            newValues: new
            {
                incident.IncidentNumber,
                incident.BusinessUnit,
                incident.SAPId,
                incident.FacilityId,
                incident.LocationId,
                Status = incident.Status.ToString(),
            },
            cancellationToken: cancellationToken);

    private async Task QueueInspectionNotificationsAsync(Incident incident, CancellationToken cancellationToken)
    {
        var recipientIds = await _identityService.GetUserIdsInRolesAsync(
            new[] { AppRoles.Inspector, AppRoles.Admin },
            cancellationToken);

        foreach (var recipientId in recipientIds)
        {
            await _notificationService.QueueNotificationAsync(new NotificationRequest
            {
                RecipientUserId = recipientId,
                Type = NotificationType.IncidentReported,
                Title = "New incident awaiting inspection",
                Message = $"Incident {incident.IncidentNumber} ({incident.BusinessUnit}) is pending inspection.",
                RelatedEntityName = nameof(Incident),
                RelatedEntityId = incident.Id,
            }, cancellationToken);
        }
    }
}
