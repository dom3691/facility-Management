using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository for the <see cref="Incident"/> aggregate.
/// </summary>
public interface IIncidentRepository : IRepository<Incident>
{
    /// <summary>
    /// Counts incidents whose <see cref="Incident.IncidentNumber"/> starts with the given
    /// prefix, ignoring the soft-delete filter so reference numbers are never reused.
    /// </summary>
    Task<int> CountByNumberPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>Loads an incident with facility, location and attachments for a detail view.</summary>
    Task<Incident?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of incidents (with facility/location) ordered newest-first, optionally
    /// filtered by reporter and/or status, together with the total matching count.
    /// </summary>
    Task<(IReadOnlyList<Incident> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? reportedByUserId,
        IncidentStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns incident counts grouped by status for a dashboard. Scope to <paramref name="reportedByUserId"/>
    /// (initiator view) or to incidents that have a work order for <paramref name="assignedVendorId"/>
    /// (vendor view); both null returns all (admin/inspector view).
    /// </summary>
    Task<IReadOnlyDictionary<IncidentStatus, int>> GetStatusCountsAsync(
        Guid? reportedByUserId,
        Guid? assignedVendorId,
        CancellationToken cancellationToken = default);
}
