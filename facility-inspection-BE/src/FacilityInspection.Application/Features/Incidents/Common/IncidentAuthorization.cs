using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Incidents.Common;

/// <summary>
/// Resource-based (object-level) authorization for reading a single incident.
/// </summary>
public static class IncidentAuthorization
{
    /// <summary>
    /// Throws <see cref="ForbiddenAccessException"/> unless the caller may view the incident:
    /// <list type="bullet">
    ///   <item><description>Admin / Inspector — any incident (they triage the whole queue).</description></item>
    ///   <item><description>Initiator — only incidents they reported.</description></item>
    ///   <item><description>Vendor — only incidents they are actively working (i.e. hold a work
    ///   order for), so they get job context without being able to browse unrelated incidents.</description></item>
    /// </list>
    /// </summary>
    public static async Task EnsureCanViewAsync(
        Incident incident,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsAdminOrInspector())
        {
            return;
        }

        var userId = currentUser.GetUserId();

        if (currentUser.IsInRole(AppRoles.Initiator) && incident.ReportedByUserId == userId)
        {
            return;
        }

        if (currentUser.IsInRole(AppRoles.Vendor) && userId is { } vendorUserId)
        {
            var vendorId = await identityService.GetUserVendorIdAsync(vendorUserId, cancellationToken);
            if (vendorId is not null)
            {
                var workOrders = await unitOfWork.WorkOrders.GetByIncidentIdAsync(incident.Id, cancellationToken);
                if (workOrders.Any(w => w.VendorId == vendorId))
                {
                    return;
                }
            }
        }

        throw new ForbiddenAccessException("You do not have permission to view this incident.");
    }
}
