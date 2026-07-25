using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="Inspection"/> aggregate.</summary>
public interface IInspectionRepository : IRepository<Inspection>
{
    /// <summary>Loads an inspection with its incident and attachments for a detail view.</summary>
    Task<Inspection?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all inspections for an incident (with attachments), newest first.</summary>
    Task<IReadOnlyList<Inspection>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken = default);

    /// <summary>Returns a page of inspections performed by an inspector, with the total count.</summary>
    Task<(IReadOnlyList<Inspection> Items, int TotalCount)> GetPagedByInspectorAsync(
        Guid inspectorUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
