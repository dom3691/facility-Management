using FacilityInspection.Domain.Common;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>
/// Coordinates work across repositories that share a single persistence context and
/// commits it atomically. Exposes typed repositories for the main aggregates plus a
/// generic accessor for any other entity type.
/// </summary>
public interface IUnitOfWork
{
    IFacilityRepository Facilities { get; }
    IIncidentRepository Incidents { get; }
    IInspectionRepository Inspections { get; }
    IVendorRepository Vendors { get; }
    IVendorAssignmentRepository VendorAssignments { get; }
    IWorkOrderRepository WorkOrders { get; }
    IVendorUpdateRepository VendorUpdates { get; }
    IVerificationRepository Verifications { get; }
    INotificationRepository Notifications { get; }
    IAuditLogRepository AuditLogs { get; }

    /// <summary>Generic repository for entity types without a dedicated repository.</summary>
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

    /// <summary>Persists all pending changes across the tracked repositories in one transaction.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
