using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using FacilityInspection.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Repositories;

/// <summary>EF Core repository for the <see cref="Notification"/> entity.</summary>
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? recipientUserId,
        bool? isRead,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        if (recipientUserId is { } userId)
        {
            query = query.Where(x => x.RecipientUserId == userId);
        }

        if (isRead is { } read)
        {
            query = query.Where(x => x.IsRead == read);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Notification>> GetPendingAsync(
        int maxBatchSize,
        CancellationToken cancellationToken = default)
        // Tracked (no AsNoTracking) so the dispatch job can update and persist them.
        => await Set
            .Where(x => x.Status == NotificationStatus.Pending)
            .OrderBy(x => x.CreatedDate)
            .Take(maxBatchSize)
            .ToListAsync(cancellationToken);
}
