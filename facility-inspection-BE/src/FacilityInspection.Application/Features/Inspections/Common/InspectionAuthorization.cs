using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Inspections.Common;

/// <summary>
/// Resource-based access checks for inspections:
/// Admin sees all; the performing inspector sees their own; the incident's initiator
/// sees inspections on their own incident.
/// </summary>
public static class InspectionAuthorization
{
    public static void EnsureCanView(Inspection inspection, ICurrentUserService currentUser)
    {
        var userId = ParseUserId(currentUser);

        if (currentUser.Roles.Contains(AppRoles.Admin))
        {
            return;
        }

        if (inspection.InspectorUserId == userId)
        {
            return;
        }

        if (inspection.Incident is not null && inspection.Incident.ReportedByUserId == userId)
        {
            return;
        }

        throw new ForbiddenAccessException("You do not have permission to view this inspection.");
    }

    public static void EnsureCanViewIncidentInspections(Incident incident, ICurrentUserService currentUser)
    {
        var userId = ParseUserId(currentUser);

        if (currentUser.Roles.Contains(AppRoles.Admin))
        {
            return;
        }

        // Inspectors work the queue, so they may review any incident's inspections.
        if (currentUser.Roles.Contains(AppRoles.Inspector))
        {
            return;
        }

        if (incident.ReportedByUserId == userId)
        {
            return;
        }

        throw new ForbiddenAccessException("You do not have permission to view these inspections.");
    }

    private static Guid ParseUserId(ICurrentUserService currentUser)
        => Guid.TryParse(currentUser.UserId, out var id) ? id : Guid.Empty;
}
