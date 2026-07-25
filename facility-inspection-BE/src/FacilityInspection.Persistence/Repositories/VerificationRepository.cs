using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="Verification"/> aggregate.</summary>
public class VerificationRepository : Repository<Verification>, IVerificationRepository
{
    public VerificationRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<Verification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.WorkOrder)
                .ThenInclude(w => w.Incident)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Verification>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Include(x => x.WorkOrder)
                .ThenInclude(w => w.Incident)
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.VerificationDate)
            .ToListAsync(cancellationToken);
}
