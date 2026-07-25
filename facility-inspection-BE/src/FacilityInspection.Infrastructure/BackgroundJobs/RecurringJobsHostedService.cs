using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Infrastructure.BackgroundJobs;

/// <summary>
/// Registers the application's Hangfire recurring jobs at startup. Runs as a hosted service
/// (only when the host actually starts), so it never fires during EF design-time tooling.
/// Failures are logged, not fatal.
/// </summary>
public class RecurringJobsHostedService : IHostedService
{
    public const string SendPendingNotificationsJobId = "send-pending-notifications";

    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<RecurringJobsHostedService> _logger;

    public RecurringJobsHostedService(
        IRecurringJobManager recurringJobManager,
        ILogger<RecurringJobsHostedService> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _recurringJobManager.AddOrUpdate<NotificationBackgroundJob>(
                SendPendingNotificationsJobId,
                job => job.DispatchPendingAsync(),
                Cron.Minutely());

            _logger.LogInformation("Recurring job '{JobId}' registered.", SendPendingNotificationsJobId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not register recurring jobs — is Hangfire storage reachable?");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
