using FacilityInspection.Application.Common.Audit;
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

namespace FacilityInspection.Application.Features.WorkOrders.GenerateWorkOrder;

/// <summary>
/// Generates a work order from a vendor assignment: validates the workflow state, allocates
/// the work-order number, transitions the incident to <see cref="IncidentStatus.WorkOrderCreated"/>,
/// writes an audit-log entry and queues notifications — all in one transaction.
/// </summary>
public class GenerateWorkOrderCommandHandler
    : IRequestHandler<GenerateWorkOrderCommand, WorkOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<GenerateWorkOrderCommandHandler> _logger;

    public GenerateWorkOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<GenerateWorkOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<WorkOrderResponse> Handle(GenerateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUser.UserId, out var actingUserId);

        // Rule 1: vendor assignment must exist.
        var assignment = await _unitOfWork.VendorAssignments.GetByIdAsync(request.VendorAssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(VendorAssignment), request.VendorAssignmentId);

        // Rule 2: incident must exist.
        var incident = await _unitOfWork.Incidents.GetByIdAsync(assignment.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), assignment.IncidentId);

        // Rule 3: incident must be in VendorAssigned status.
        if (incident.Status != IncidentStatus.VendorAssigned)
        {
            throw new BadRequestException(
                $"Incident {incident.IncidentNumber} is not ready for a work order (status: {incident.Status}).");
        }

        // Rule 7: no duplicate work order per assignment.
        if (await _unitOfWork.WorkOrders.ExistsForVendorAssignmentAsync(assignment.Id, cancellationToken))
        {
            throw new BadRequestException("A work order already exists for this vendor assignment.");
        }

        var workOrder = new WorkOrder
        {
            WorkOrderNumber = await GenerateWorkOrderNumberAsync(cancellationToken),
            IncidentId = incident.Id,
            VendorAssignmentId = assignment.Id,
            VendorId = assignment.VendorId,
            Status = WorkOrderStatus.Assigned, // Rule 5
            Description = string.IsNullOrWhiteSpace(request.Description) ? incident.Description : request.Description,
        };

        await _unitOfWork.WorkOrders.AddAsync(workOrder, cancellationToken);

        // Rule 6: incident becomes WorkOrderCreated.
        incident.Status = IncidentStatus.WorkOrderCreated;

        // Rule 8: audit log.
        await _auditService.LogWorkflowActionAsync(
            nameof(WorkOrder),
            workOrder.Id.ToString(),
            AuditActions.WorkOrderGenerated,
            newValues: new
            {
                workOrder.WorkOrderNumber,
                workOrder.IncidentId,
                incident.IncidentNumber,
                workOrder.VendorId,
                Status = workOrder.Status.ToString(),
            },
            cancellationToken: cancellationToken);

        // Rule 9: notify the incident initiator and the vendor's users.
        await QueueNotificationsAsync(workOrder, incident, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Work order {WorkOrderNumber} generated for incident {IncidentNumber} by {UserId}",
            workOrder.WorkOrderNumber, incident.IncidentNumber, actingUserId);

        var created = await _unitOfWork.WorkOrders.GetByIdWithDetailsAsync(workOrder.Id, cancellationToken);
        return created!.ToResponse();
    }

    private async Task<string> GenerateWorkOrderNumberAsync(CancellationToken cancellationToken)
    {
        var year = _dateTime.UtcNow.Year;
        var prefix = $"WO-{year}-";
        var count = await _unitOfWork.WorkOrders.CountByNumberPrefixAsync(prefix, cancellationToken);
        return $"{prefix}{count + 1:D6}";
    }

    private async Task QueueNotificationsAsync(WorkOrder workOrder, Incident incident, CancellationToken cancellationToken)
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
                Type = NotificationType.WorkOrderCreated,
                Title = "Work order generated",
                Message = $"Work order {workOrder.WorkOrderNumber} was generated for incident {incident.IncidentNumber}.",
                RelatedEntityName = nameof(WorkOrder),
                RelatedEntityId = workOrder.Id,
            }, cancellationToken);
        }
    }
}
