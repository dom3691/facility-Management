using FacilityInspection.Application.Common.Interfaces;

namespace FacilityInspection.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire job that dispatches pending notifications. Resolved from DI per execution;
/// delegates to <see cref="INotificationService.SendPendingNotificationsAsync"/>.
/// </summary>
public class NotificationBackgroundJob
{
    private readonly INotificationService _notificationService;

    public NotificationBackgroundJob(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task DispatchPendingAsync() => _notificationService.SendPendingNotificationsAsync();
}
