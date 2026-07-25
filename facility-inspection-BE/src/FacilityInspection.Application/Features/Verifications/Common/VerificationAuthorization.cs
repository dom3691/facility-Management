using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Verifications.Common;

/// <summary>
/// Access checks for viewing verifications:
/// Admin/Inspector see all; the vendor on the work order and the incident's initiator see their own.
/// </summary>
public static class VerificationAuthorization
{
    public static async Task EnsureCanViewAsync(
        WorkOrder workOrder,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        if (currentUser.Roles.Contains(AppRoles.Admin) || currentUser.Roles.Contains(AppRoles.Inspector))
        {
            return;
        }

        Guid.TryParse(currentUser.UserId, out var userId);

        if (currentUser.Roles.Contains(AppRoles.Initiator)
            && workOrder.Incident is not null
            && workOrder.Incident.ReportedByUserId == userId)
        {
            return;
        }

        if (currentUser.Roles.Contains(AppRoles.Vendor))
        {
            var vendorId = await identityService.GetUserVendorIdAsync(userId, cancellationToken);
            if (vendorId == workOrder.VendorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException("You do not have permission to view this verification.");
    }
}
