using FacilityInspection.Application.Common.Models;

namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Raises and dispatches user-facing notifications for workflow events. Implemented in
/// Persistence (persists Notification records; dispatch delegates to <see cref="IEmailService"/>).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a Pending notification record. Enlists in the caller's unit of work (does NOT
    /// save) so it commits atomically with the business change that raised it.
    /// </summary>
    Task QueueNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches Pending notifications via <see cref="IEmailService"/>, marking each Sent or
    /// Failed. Invoked by the Hangfire background job; commits its own transaction.
    /// </summary>
    Task SendPendingNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Marks a notification read for the given user (ownership enforced).</summary>
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}
