using System.Collections.Concurrent;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Common;
using FacilityInspection.Persistence.Context;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>. All repositories it exposes share
/// the same scoped <see cref="ApplicationDbContext"/>, so a single
/// <see cref="SaveChangesAsync"/> commits their combined changes in one transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _genericRepositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Facilities = new FacilityRepository(context);
        Incidents = new IncidentRepository(context);
        Inspections = new InspectionRepository(context);
        Vendors = new VendorRepository(context);
        VendorAssignments = new VendorAssignmentRepository(context);
        WorkOrders = new WorkOrderRepository(context);
        VendorUpdates = new VendorUpdateRepository(context);
        Verifications = new VerificationRepository(context);
        Notifications = new NotificationRepository(context);
        AuditLogs = new AuditLogRepository(context);
    }

    public IFacilityRepository Facilities { get; }
    public IIncidentRepository Incidents { get; }
    public IInspectionRepository Inspections { get; }
    public IVendorRepository Vendors { get; }
    public IVendorAssignmentRepository VendorAssignments { get; }
    public IWorkOrderRepository WorkOrders { get; }
    public IVendorUpdateRepository VendorUpdates { get; }
    public IVerificationRepository Verifications { get; }
    public INotificationRepository Notifications { get; }
    public IAuditLogRepository AuditLogs { get; }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        => (IRepository<TEntity>)_genericRepositories.GetOrAdd(
            typeof(TEntity),
            _ => new Repository<TEntity>(_context));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
