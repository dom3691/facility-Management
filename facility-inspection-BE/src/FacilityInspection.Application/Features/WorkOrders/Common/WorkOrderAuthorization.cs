using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.WorkOrders.Common;

/// <summary>
/// Resource-based (object-level) authorization for reading a single work order. Role attributes
/// on the controller gate <em>which roles</em> may call an endpoint; this gate decides whether a
/// caller may see <em>this particular</em> work order.
/// </summary>
public static class WorkOrderAuthorization
{
    /// <summary>
    /// Throws <see cref="ForbiddenAccessException"/> unless the caller may view the work order:
    /// <list type="bullet">
    ///   <item><description>Admin / Inspector — any work order (operational oversight).</description></item>
    ///   <item><description>Initiator — only work orders on an incident they reported.</description></item>
    ///   <item><description>Vendor — only work orders assigned to their own vendor. This is the
    ///   check that stops one vendor from reading another vendor's work orders.</description></item>
    /// </list>
    /// The work order's <see cref="WorkOrder.Incident"/> must be loaded for the Initiator branch.
    /// </summary>
    public static async Task EnsureCanViewAsync(
        WorkOrder workOrder,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsAdminOrInspector())
        {
            return;
        }

        var userId = currentUser.GetUserId();

        if (currentUser.IsInRole(AppRoles.Initiator)
            && workOrder.Incident is not null
            && workOrder.Incident.ReportedByUserId == userId)
        {
            return;
        }

        if (currentUser.IsInRole(AppRoles.Vendor) && userId is { } vendorUserId)
        {
            var vendorId = await identityService.GetUserVendorIdAsync(vendorUserId, cancellationToken);
            if (vendorId is not null && vendorId == workOrder.VendorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException("You do not have permission to view this work order.");
    }
}
