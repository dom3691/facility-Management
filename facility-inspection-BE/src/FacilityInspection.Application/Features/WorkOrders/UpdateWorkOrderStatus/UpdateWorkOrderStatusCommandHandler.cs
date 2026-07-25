using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Features.WorkOrders.UpdateWorkOrderStatus;

/// <summary>
/// Advances a work order through its allowed transitions:
/// Assigned → InProgress → Completed. <c>Completed</c> awaits verification, and
/// <c>Closed</c> is reached only via the Verification module.
/// </summary>
public class UpdateWorkOrderStatusCommandHandler
    : IRequestHandler<UpdateWorkOrderStatusCommand, WorkOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<UpdateWorkOrderStatusCommandHandler> _logger;

    public UpdateWorkOrderStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<UpdateWorkOrderStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<WorkOrderResponse> Handle(
        UpdateWorkOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUser.UserId, out var actingUserId);

        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        await EnsureCanUpdateAsync(workOrder, actingUserId, cancellationToken);
        EnsureTransitionAllowed(workOrder.Status, request.Status);

        var previousStatus = workOrder.Status;
        workOrder.Status = request.Status;
        if (request.Status == WorkOrderStatus.Completed)
        {
            workOrder.CompletedDate = _dateTime.UtcNow;
        }

        await _auditService.LogWorkflowActionAsync(
            nameof(WorkOrder),
            workOrder.Id.ToString(),
            AuditActions.WorkOrderStatusChanged,
            oldValues: new { Status = previousStatus.ToString() },
            newValues: new { workOrder.WorkOrderNumber, Status = request.Status.ToString() },
            cancellationToken: cancellationToken);

        // When completed, notify admins that verification is required.
        if (request.Status == WorkOrderStatus.Completed)
        {
            var adminIds = await _identityService.GetUserIdsInRolesAsync(new[] { AppRoles.Admin }, cancellationToken);
            foreach (var adminId in adminIds)
            {
                await _notificationService.QueueNotificationAsync(new NotificationRequest
                {
                    RecipientUserId = adminId,
                    Type = NotificationType.WorkOrderCompleted,
                    Title = "Work order completed",
                    Message = $"Work order {workOrder.WorkOrderNumber} is completed and awaiting verification.",
                    RelatedEntityName = nameof(WorkOrder),
                    RelatedEntityId = workOrder.Id,
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Work order {WorkOrderNumber} status {Previous} → {New} by {UserId}",
            workOrder.WorkOrderNumber, previousStatus, request.Status, actingUserId);

        var updated = await _unitOfWork.WorkOrders.GetByIdWithDetailsAsync(workOrder.Id, cancellationToken);
        return updated!.ToResponse();
    }

    private async Task EnsureCanUpdateAsync(WorkOrder workOrder, Guid userId, CancellationToken cancellationToken)
    {
        if (_currentUser.Roles.Contains(AppRoles.Admin))
        {
            return;
        }

        // A vendor user may only update work orders belonging to their own vendor.
        if (_currentUser.Roles.Contains(AppRoles.Vendor))
        {
            var vendorId = await _identityService.GetUserVendorIdAsync(userId, cancellationToken);
            if (vendorId == workOrder.VendorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException("You do not have permission to update this work order.");
    }

    private static void EnsureTransitionAllowed(WorkOrderStatus current, WorkOrderStatus target)
    {
        if (target == WorkOrderStatus.Closed)
        {
            throw new BadRequestException("A work order is closed only through the Verification module.");
        }

        var allowed = (current, target) switch
        {
            (WorkOrderStatus.Assigned, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
            _ => false,
        };

        if (!allowed)
        {
            throw new BadRequestException(
                $"Invalid status transition {current} → {target}. Allowed: Assigned → InProgress → Completed.");
        }
    }
}
