using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Application.Features.Verifications.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.Verifications.CreateVerification;

/// <summary>
/// Records the initiator's verification of a completed work order. <c>Fixed</c> closes the
/// work order and incident; <c>NotFixed</c> returns the work to the vendor. Writes an
/// audit-log entry and queues notifications — all in one transaction.
/// </summary>
public class CreateVerificationCommandHandler
    : IRequestHandler<CreateVerificationCommand, VerificationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateVerificationCommandHandler> _logger;

    public CreateVerificationCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<CreateVerificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<VerificationResponse> Handle(CreateVerificationCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var verifiedByUserId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        // Rule 1: work order must exist.
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Rule 2: work order must be Completed.
        if (workOrder.Status != WorkOrderStatus.Completed)
        {
            throw new BadRequestException(
                $"Work order {workOrder.WorkOrderNumber} is not awaiting verification (status: {workOrder.Status}).");
        }

        // Rule 3: incident must exist.
        var incident = await _unitOfWork.Incidents.GetByIdAsync(workOrder.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), workOrder.IncidentId);

        // Rule 4: only the incident's initiator or an Admin may verify.
        var isAdmin = _currentUser.Roles.Contains(AppRoles.Admin);
        if (!isAdmin && incident.ReportedByUserId != verifiedByUserId)
        {
            throw new ForbiddenAccessException("Only the incident's initiator or an administrator can verify this work.");
        }

        var verification = new Verification
        {
            WorkOrderId = workOrder.Id,
            VerifiedByUserId = verifiedByUserId,
            VerificationDate = _dateTime.UtcNow,
            Decision = request.Decision,
            Remarks = request.Comments,
        };

        await _unitOfWork.Verifications.AddAsync(verification, cancellationToken);

        if (request.Decision == VerificationDecision.Fixed)
        {
            workOrder.Status = WorkOrderStatus.Closed;
            incident.Status = IncidentStatus.Closed;
            await WriteAuditAsync(verification, workOrder, incident, AuditActions.VerificationFixed, cancellationToken);
            await NotifyAsync(
                workOrder, incident,
                "Verification completed",
                $"Work order {workOrder.WorkOrderNumber} was verified as Fixed and closed.",
                cancellationToken);
        }
        else
        {
            // Returned to the vendor for rework.
            workOrder.Status = WorkOrderStatus.InProgress;
            incident.Status = IncidentStatus.WorkOrderCreated;
            await WriteAuditAsync(verification, workOrder, incident, AuditActions.VerificationNotFixed, cancellationToken);
            await NotifyAsync(
                workOrder, incident,
                "Work returned to vendor",
                $"Work order {workOrder.WorkOrderNumber} was verified as Not Fixed and returned for rework.",
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Verification {VerificationId} ({Decision}) recorded for work order {WorkOrderNumber} by {UserId}",
            verification.Id, request.Decision, workOrder.WorkOrderNumber, verifiedByUserId);

        var created = await _unitOfWork.Verifications.GetByIdWithDetailsAsync(verification.Id, cancellationToken);
        return created!.ToResponse();
    }

    private Task WriteAuditAsync(
        Verification verification,
        WorkOrder workOrder,
        Incident incident,
        string action,
        CancellationToken cancellationToken)
        => _auditService.LogWorkflowActionAsync(
            nameof(Verification),
            verification.Id.ToString(),
            action,
            oldValues: new { WorkOrderStatus = WorkOrderStatus.Completed.ToString() },
            newValues: new
            {
                Decision = verification.Decision.ToString(),
                workOrder.WorkOrderNumber,
                WorkOrderStatus = workOrder.Status.ToString(),
                incident.IncidentNumber,
                IncidentStatus = incident.Status.ToString(),
                verification.Remarks,
            },
            cancellationToken: cancellationToken);

    private async Task NotifyAsync(
        WorkOrder workOrder,
        Incident incident,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        var recipients = new HashSet<Guid> { incident.ReportedByUserId };

        foreach (var vendorUserId in await _identityService.GetUserIdsByVendorIdAsync(workOrder.VendorId, cancellationToken))
        {
            recipients.Add(vendorUserId);
        }

        foreach (var recipientId in recipients)
        {
            await _notificationService.QueueNotificationAsync(new NotificationRequest
            {
                RecipientUserId = recipientId,
                Type = NotificationType.VerificationCompleted,
                Title = title,
                Message = message,
                RelatedEntityName = nameof(WorkOrder),
                RelatedEntityId = workOrder.Id,
            }, cancellationToken);
        }
    }
}
