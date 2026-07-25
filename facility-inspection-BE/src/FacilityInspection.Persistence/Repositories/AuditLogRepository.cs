using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="AuditLog"/> trail.</summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? entityName,
        string? action,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action == action);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.PerformedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Where(x => x.EntityName == entityName && x.EntityId == entityId)
            .OrderByDescending(x => x.PerformedDate)
            .ToListAsync(cancellationToken);
}
