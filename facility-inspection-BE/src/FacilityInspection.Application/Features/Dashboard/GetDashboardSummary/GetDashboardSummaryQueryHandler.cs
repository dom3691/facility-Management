using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Dashboard;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>
/// Builds the dashboard counters, scoping the underlying counts to the caller's role:
/// Admin/Inspector see everything; an Initiator sees their own incidents (and the work orders
/// for them); a Vendor sees their own work orders (and the incidents they are working on).
/// </summary>
public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetDashboardSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<DashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUser.UserId, out var userId);
        var roles = _currentUser.Roles;

        Guid? incidentReporter = null;
        Guid? incidentVendor = null;
        Guid? workOrderVendor = null;
        Guid? workOrderInitiator = null;

        if (roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Inspector))
        {
            // Full, unscoped view.
        }
        else if (roles.Contains(AppRoles.Vendor))
        {
            // Guid.Empty matches nothing when the user is not linked to a vendor.
            var vendorId = await _identityService.GetUserVendorIdAsync(userId, cancellationToken) ?? Guid.Empty;
            incidentVendor = vendorId;
            workOrderVendor = vendorId;
        }
        else
        {
            // Initiator (default): own incidents and their work orders.
            incidentReporter = userId;
            workOrderInitiator = userId;
        }

        var incidentCounts = await _unitOfWork.Incidents
            .GetStatusCountsAsync(incidentReporter, incidentVendor, cancellationToken);

        var workOrderCounts = await _unitOfWork.WorkOrders
            .GetStatusCountsAsync(workOrderVendor, workOrderInitiator, cancellationToken);

        return new DashboardSummaryResponse
        {
            TotalIncidents = incidentCounts.Values.Sum(),
            PendingInspectionCount = incidentCounts.GetValueOrDefault(IncidentStatus.PendingInspection),
            // "Inspection completed" = inspected with a fault, now awaiting vendor assignment.
            InspectionCompletedCount = incidentCounts.GetValueOrDefault(IncidentStatus.AwaitingVendorAssignment),
            VendorAssignedCount = incidentCounts.GetValueOrDefault(IncidentStatus.VendorAssigned),
            PendingVerificationCount = incidentCounts.GetValueOrDefault(IncidentStatus.AwaitingVerification),
            WorkOrdersOpenCount = workOrderCounts.GetValueOrDefault(WorkOrderStatus.Open),
            WorkOrdersInProgressCount = workOrderCounts.GetValueOrDefault(WorkOrderStatus.InProgress),
            WorkOrdersCompletedCount = workOrderCounts.GetValueOrDefault(WorkOrderStatus.Completed),
            WorkOrdersClosedCount = workOrderCounts.GetValueOrDefault(WorkOrderStatus.Closed),
        };
    }
}
