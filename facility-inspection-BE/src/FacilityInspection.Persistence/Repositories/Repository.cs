using System.Linq.Expressions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Common;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{TEntity}"/>. Soft-delete filtering is
/// applied automatically by the global query filter; audit fields are stamped by the
/// save-changes interceptor.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> Set;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(TEntity entity) => Set.Update(entity);

    public virtual void Delete(TEntity entity) => Set.Remove(entity);

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}
