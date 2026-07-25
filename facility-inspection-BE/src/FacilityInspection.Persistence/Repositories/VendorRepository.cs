using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="Vendor"/> aggregate.</summary>
public class VendorRepository : Repository<Vendor>, IVendorRepository
{
    public VendorRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<Vendor> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        VendorCategory? category,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        if (category is { } vendorCategory)
        {
            query = query.Where(x => x.Category == vendorCategory);
        }

        if (isActive is { } active)
        {
            query = query.Where(x => x.IsActive == active);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
