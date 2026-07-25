using System.Linq.Expressions;
using FacilityInspection.Domain.Common;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>
/// Generic persistence-ignorant repository abstraction over an aggregate/entity type.
/// Declared in the Application layer so use cases depend on the contract, not on EF Core;
/// the implementation lives in the Persistence layer.
/// </summary>
/// <typeparam name="TEntity">A domain entity type.</typeparam>
public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    /// <summary>
    /// Removes the entity. For <see cref="ISoftDelete"/> types this is converted into a
    /// soft-delete flag update by the persistence interceptor.
    /// </summary>
    void Delete(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
