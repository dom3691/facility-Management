using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>
/// EF Core repository for the <see cref="Incident"/> aggregate.
/// </summary>
public class IncidentRepository : Repository<Incident>, IIncidentRepository
{
    public IncidentRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<int> CountByNumberPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Set
            .IgnoreQueryFilters() // include soft-deleted so numbers are never reused
            .CountAsync(x => x.IncidentNumber.StartsWith(prefix), cancellationToken);

    public Task<Incident?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.Facility)
            .Include(x => x.Location)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Incident> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? reportedByUserId,
        IncidentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .AsNoTracking()
            .Include(x => x.Facility)
            .Include(x => x.Location)
            .AsQueryable();

        if (reportedByUserId is { } userId)
        {
            query = query.Where(x => x.ReportedByUserId == userId);
        }

        if (status is { } incidentStatus)
        {
            query = query.Where(x => x.Status == incidentStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyDictionary<IncidentStatus, int>> GetStatusCountsAsync(
        Guid? reportedByUserId,
        Guid? assignedVendorId,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        if (reportedByUserId is { } reporter)
        {
            query = query.Where(i => i.ReportedByUserId == reporter);
        }

        if (assignedVendorId is { } vendorId)
        {
            query = query.Where(i =>
                Context.Set<WorkOrder>().Any(w => w.IncidentId == i.Id && w.VendorId == vendorId));
        }

        return await query
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
