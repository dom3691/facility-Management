using FacilityInspection.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Infrastructure.Services;

/// <summary>
/// Placeholder <see cref="IEmailService"/> that logs instead of sending, so the
/// foundation runs without an SMTP/provider dependency. Swap for a real provider
/// (SendGrid, Graph, SMTP) when the notification feature is built.
/// </summary>
public class LoggingEmailSender : IEmailService
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email (logging sender — not actually sent). To: {To}, Subject: {Subject}, Body: {Body}",
            to, subject, body);
        return Task.CompletedTask;
    }
}
