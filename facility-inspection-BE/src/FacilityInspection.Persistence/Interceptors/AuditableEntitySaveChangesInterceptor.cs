using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FacilityInspection.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that (1) stamps created/modified audit metadata on
/// <see cref="IAuditableEntity"/> instances and (2) converts hard deletes of
/// <see cref="ISoftDelete"/> instances into flag updates.
/// </summary>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUser, IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyChanges(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = _dateTime.UtcNow;
        var userId = _currentUser.UserId;

        // Turn deletes of soft-delete entities into updates first, so the audit pass
        // below stamps ModifiedBy/ModifiedDate on them as well.
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedBy = userId;
                entry.Entity.DeletedDate = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.ModifiedBy = userId;
                entry.Entity.ModifiedDate = now;
            }
        }
    }
}
