using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="VendorAssignment"/> aggregate.</summary>
public interface IVendorAssignmentRepository : IRepository<VendorAssignment>
{
    /// <summary>Loads an assignment with its vendor, incident and inspection for a detail view.</summary>
    Task<VendorAssignment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all assignments for an incident (with vendor), newest first.</summary>
    Task<IReadOnlyList<VendorAssignment>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default);
}
