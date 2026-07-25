using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>
/// EF Core repository for the <see cref="Inspection"/> aggregate.
/// </summary>
public class InspectionRepository : Repository<Inspection>, IInspectionRepository
{
    public InspectionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<Inspection?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Inspection>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Attachments)
            .Where(x => x.IncidentId == incidentId)
            .OrderByDescending(x => x.InspectionDate)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Inspection> Items, int TotalCount)> GetPagedByInspectorAsync(
        Guid inspectorUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Attachments)
            .Where(x => x.InspectorUserId == inspectorUserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
