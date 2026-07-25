using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Files;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorUpdates;
using FacilityInspection.Application.Features.VendorUpdates.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.VendorUpdates.MarkWorkOrderComplete;

/// <summary>
/// Marks a work order complete: records a completion update (with evidence), moves the work
/// order to <c>Completed</c> and the incident to <c>AwaitingVerification</c>, writes an
/// audit-log entry and queues notifications — all in one transaction.
/// </summary>
public class MarkWorkOrderCompleteCommandHandler
    : IRequestHandler<MarkWorkOrderCompleteCommand, VendorUpdateResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<MarkWorkOrderCompleteCommandHandler> _logger;

    public MarkWorkOrderCompleteCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<MarkWorkOrderCompleteCommandHandler> logger)
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

    public async Task<VendorUpdateResponse> Handle(
        MarkWorkOrderCompleteCommand request,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUser.UserId, out var updatedByUserId);

        // Rule 1: work order must exist.
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Rule 2: vendor may only complete their own work orders.
        await VendorUpdateAuthorization.EnsureCanAccessWorkOrderAsync(
            workOrder, _currentUser, _identityService, cancellationToken);

        // Rule 3: work order must be Assigned or InProgress.
        if (workOrder.Status is not (WorkOrderStatus.Assigned or WorkOrderStatus.InProgress))
        {
            throw new BadRequestException(
                $"Work order {workOrder.WorkOrderNumber} cannot be completed (status: {workOrder.Status}).");
        }

        var incident = await _unitOfWork.Incidents.GetByIdAsync(workOrder.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), workOrder.IncidentId);

        var update = new VendorUpdate
        {
            WorkOrderId = workOrder.Id,
            UpdatedByUserId = updatedByUserId,
            UpdateDate = _dateTime.UtcNow,
            StatusAtUpdate = WorkOrderStatus.Completed,
            PercentComplete = 100,
            IsCompletionUpdate = true,
            CompletionComment = request.CompletionComment,
        };

        await StoreAttachmentsAsync(update, request.Attachments, cancellationToken);
        await _unitOfWork.VendorUpdates.AddAsync(update, cancellationToken);

        // Rule 5: transition the work order and incident.
        workOrder.Status = WorkOrderStatus.Completed;
        workOrder.CompletedDate = _dateTime.UtcNow;
        incident.Status = IncidentStatus.AwaitingVerification;

        // Rule 5: audit log.
        await _auditService.LogWorkflowActionAsync(
            nameof(WorkOrder),
            workOrder.Id.ToString(),
            AuditActions.VendorMarkedComplete,
            newValues: new
            {
                workOrder.WorkOrderNumber,
                WorkOrderStatus = workOrder.Status.ToString(),
                incident.IncidentNumber,
                IncidentStatus = incident.Status.ToString(),
                update.CompletionComment,
            },
            cancellationToken: cancellationToken);

        // Rule 5: notify admins (for verification) and the incident initiator.
        await QueueNotificationsAsync(workOrder, incident, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Work order {WorkOrderNumber} marked complete by {UserId}; incident {IncidentNumber} → AwaitingVerification",
            workOrder.WorkOrderNumber, updatedByUserId, incident.IncidentNumber);

        var created = await _unitOfWork.VendorUpdates.GetByIdWithDetailsAsync(update.Id, cancellationToken);
        return created!.ToResponse();
    }

    private async Task StoreAttachmentsAsync(
        VendorUpdate update,
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
                stream, file.FileName, file.ContentType, FileModules.VendorUpdates, cancellationToken);

            update.Attachments.Add(new VendorUpdateAttachment
            {
                FileName = file.FileName,
                StoragePath = storagePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.SizeBytes,
            });
        }
    }

    private async Task QueueNotificationsAsync(WorkOrder workOrder, Incident incident, CancellationToken cancellationToken)
    {
        var recipients = new HashSet<Guid> { incident.ReportedByUserId };

        foreach (var adminId in await _identityService.GetUserIdsInRolesAsync(new[] { AppRoles.Admin }, cancellationToken))
        {
            recipients.Add(adminId);
        }

        foreach (var recipientId in recipients)
        {
            await _notificationService.QueueNotificationAsync(new NotificationRequest
            {
                RecipientUserId = recipientId,
                Type = NotificationType.WorkOrderCompleted,
                Title = "Work order completed by vendor",
                Message = $"Work order {workOrder.WorkOrderNumber} for incident {incident.IncidentNumber} is complete and awaiting verification.",
                RelatedEntityName = nameof(WorkOrder),
                RelatedEntityId = workOrder.Id,
            }, cancellationToken);
        }
    }
}
