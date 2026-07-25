using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Repository for the <see cref="Notification"/> entity.</summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>
    /// Returns a page of notifications (newest first), optionally filtered by recipient and
    /// read state, with the total count.
    /// </summary>
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? recipientUserId,
        bool? isRead,
        CancellationToken cancellationToken = default);

    /// <summary>Returns up to <paramref name="maxBatchSize"/> tracked Pending notifications to dispatch.</summary>
    Task<IReadOnlyList<Notification>> GetPendingAsync(
        int maxBatchSize,
        CancellationToken cancellationToken = default);
}
