namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the persistence store consumed by Application handlers.
/// The concrete implementation lives in the Persistence layer.
/// <para>
/// Aggregate access is exposed here as features are built. To keep this layer free
/// of EF Core, prefer exposing <c>DbSet&lt;T&gt;</c> from the Persistence-side
/// implementation and either (a) reference EF Core here if you accept the coupling,
/// or (b) expose repository interfaces instead. This foundation keeps only the
/// unit-of-work commit boundary.
/// </para>
/// </summary>
public interface IApplicationDbContext
{
    // Example once EF Core coupling is accepted in Application:
    // DbSet<Incident> Incidents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
