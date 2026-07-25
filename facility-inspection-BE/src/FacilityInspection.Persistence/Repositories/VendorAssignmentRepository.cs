using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="VendorAssignment"/> aggregate.</summary>
public class VendorAssignmentRepository : Repository<VendorAssignment>, IVendorAssignmentRepository
{
    public VendorAssignmentRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<VendorAssignment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Incident)
            .Include(x => x.Inspection)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VendorAssignment>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Incident)
            .Where(x => x.IncidentId == incidentId)
            .OrderByDescending(x => x.AssignedDate)
            .ToListAsync(cancellationToken);
}
