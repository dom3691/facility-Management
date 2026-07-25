using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.VendorUpdates.Common;

/// <summary>
/// Access checks for vendor updates: Admin may act on any work order; a Vendor user only on
/// work orders belonging to their own vendor.
/// </summary>
public static class VendorUpdateAuthorization
{
    public static async Task EnsureCanAccessWorkOrderAsync(
        WorkOrder workOrder,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        if (currentUser.Roles.Contains(AppRoles.Admin))
        {
            return;
        }

        if (currentUser.Roles.Contains(AppRoles.Vendor)
            && Guid.TryParse(currentUser.UserId, out var userId))
        {
            var vendorId = await identityService.GetUserVendorIdAsync(userId, cancellationToken);
            if (vendorId == workOrder.VendorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException("You do not have permission to access this work order.");
    }
}
