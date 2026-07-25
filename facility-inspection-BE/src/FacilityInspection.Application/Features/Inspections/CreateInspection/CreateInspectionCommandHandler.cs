using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Files;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Application.Features.Inspections.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.Inspections.CreateInspection;

/// <summary>
/// Records an inspection for a pending incident: stores photo evidence, transitions the
/// incident (Good → Closed; otherwise → AwaitingVendorAssignment), writes an audit-log
/// entry and queues notifications — all in one transaction.
/// </summary>
public class CreateInspectionCommandHandler
    : IRequestHandler<CreateInspectionCommand, InspectionResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateInspectionCommandHandler> _logger;

    public CreateInspectionCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<CreateInspectionCommandHandler> logger)
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

    public async Task<InspectionResponse> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var inspectorUserId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        var incident = await _unitOfWork.Incidents.GetByIdAsync(request.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), request.IncidentId);

        if (incident.Status is not (IncidentStatus.PendingInspection or IncidentStatus.UnderInspection))
        {
            throw new BadRequestException(
                $"Incident {incident.IncidentNumber} is not awaiting inspection (status: {incident.Status}).");
        }

        var requiresVendor = request.Classification != InspectionClassification.Good;

        var inspection = new Inspection
        {
            IncidentId = incident.Id,
            InspectorUserId = inspectorUserId,
            InspectionDate = _dateTime.UtcNow,
            Classification = request.Classification,
            Findings = request.Comments,
            RequiresVendor = requiresVendor,
        };

        await StoreAttachmentsAsync(inspection, request.Attachments, cancellationToken);
        await _unitOfWork.Inspections.AddAsync(inspection, cancellationToken);

        // Transition the incident.
        incident.Status = requiresVendor
            ? IncidentStatus.AwaitingVendorAssignment // inspection completed → available for vendor assignment
            : IncidentStatus.Closed;

        await WriteAuditLogAsync(inspection, incident, cancellationToken);
        await QueueNotificationsAsync(inspection, incident, requiresVendor, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inspection {InspectionId} ({Classification}) recorded for incident {IncidentNumber} by {UserId}; incident → {Status}",
            inspection.Id, request.Classification, incident.IncidentNumber, inspectorUserId, incident.Status);

        var created = await _unitOfWork.Inspections.GetByIdWithDetailsAsync(inspection.Id, cancellationToken);
        return created!.ToResponse();
    }

    private async Task StoreAttachmentsAsync(
        Inspection inspection,
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
                stream, file.FileName, file.ContentType, FileModules.Inspections, cancellationToken);

            inspection.Attachments.Add(new InspectionAttachment
            {
                FileName = file.FileName,
                StoragePath = storagePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.SizeBytes,
            });
        }
    }

    private Task WriteAuditLogAsync(Inspection inspection, Incident incident, CancellationToken cancellationToken)
        => _auditService.LogWorkflowActionAsync(
            nameof(Inspection),
            inspection.Id.ToString(),
            AuditActions.InspectionCompleted,
            newValues: new
            {
                inspection.IncidentId,
                incident.IncidentNumber,
                Classification = inspection.Classification.ToString(),
                inspection.RequiresVendor,
                IncidentStatus = incident.Status.ToString(),
            },
            cancellationToken: cancellationToken);

    private async Task QueueNotificationsAsync(
        Inspection inspection,
        Incident incident,
        bool requiresVendor,
        CancellationToken cancellationToken)
    {
        // Notify the incident initiator that the inspection is complete.
        await AddNotificationAsync(
            incident.ReportedByUserId,
            "Inspection completed",
            requiresVendor
                ? $"Incident {incident.IncidentNumber} was inspected ({inspection.Classification}) and is awaiting vendor assignment."
                : $"Incident {incident.IncidentNumber} was inspected (Good) and has been closed.",
            incident.Id,
            cancellationToken);

        // For faulty/damaged/run-down, notify admins so vendor assignment can proceed.
        if (requiresVendor)
        {
            var adminIds = await _identityService.GetUserIdsInRolesAsync(new[] { AppRoles.Admin }, cancellationToken);
            foreach (var adminId in adminIds)
            {
                await AddNotificationAsync(
                    adminId,
                    "Incident awaiting vendor assignment",
                    $"Incident {incident.IncidentNumber} was classified {inspection.Classification} and requires vendor assignment.",
                    incident.Id,
                    cancellationToken);
            }
        }
    }

    private Task AddNotificationAsync(
        Guid recipientUserId,
        string title,
        string message,
        Guid incidentId,
        CancellationToken cancellationToken)
        => _notificationService.QueueNotificationAsync(new NotificationRequest
        {
            RecipientUserId = recipientUserId,
            Type = NotificationType.InspectionCompleted,
            Title = title,
            Message = message,
            RelatedEntityName = nameof(Incident),
            RelatedEntityId = incidentId,
        }, cancellationToken);
}
