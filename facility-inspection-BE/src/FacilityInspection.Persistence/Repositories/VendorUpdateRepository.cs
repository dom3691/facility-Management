using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="VendorUpdate"/> progress log.</summary>
public class VendorUpdateRepository : Repository<VendorUpdate>, IVendorUpdateRepository
{
    public VendorUpdateRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<VendorUpdate?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VendorUpdate>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Include(x => x.Attachments)
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.UpdateDate)
            .ToListAsync(cancellationToken);
}
