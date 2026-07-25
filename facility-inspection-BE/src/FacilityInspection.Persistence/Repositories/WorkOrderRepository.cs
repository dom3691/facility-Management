using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="WorkOrder"/> aggregate.</summary>
public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<int> CountByNumberPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Set
            .IgnoreQueryFilters() // include soft-deleted so numbers are never reused
            .CountAsync(x => x.WorkOrderNumber.StartsWith(prefix), cancellationToken);

    public Task<bool> ExistsForVendorAssignmentAsync(
        Guid vendorAssignmentId,
        CancellationToken cancellationToken = default)
        => Set.AnyAsync(x => x.VendorAssignmentId == vendorAssignmentId, cancellationToken);

    public Task<WorkOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Vendor)
            .Where(x => x.IncidentId == incidentId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        WorkOrderStatus? status,
        Guid? vendorId,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Vendor)
            .AsQueryable();

        if (status is { } workOrderStatus)
        {
            query = query.Where(x => x.Status == workOrderStatus);
        }

        if (vendorId is { } vendor)
        {
            query = query.Where(x => x.VendorId == vendor);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPendingVerificationPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? initiatorUserId,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .AsNoTracking()
            .Include(x => x.Incident)
            .Include(x => x.Vendor)
            .Where(x => x.Status == WorkOrderStatus.Completed);

        if (initiatorUserId is { } userId)
        {
            query = query.Where(x => x.Incident.ReportedByUserId == userId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CompletedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyDictionary<WorkOrderStatus, int>> GetStatusCountsAsync(
        Guid? vendorId,
        Guid? initiatorUserId,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        if (vendorId is { } vendor)
        {
            query = query.Where(w => w.VendorId == vendor);
        }

        if (initiatorUserId is { } initiator)
        {
            query = query.Where(w => w.Incident.ReportedByUserId == initiator);
        }

        return await query
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
