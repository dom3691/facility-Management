using System.Net;
using System.Net.Mail;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacilityInspection.Infrastructure.Services;

/// <summary>SMTP-backed email sender. Activated when <c>Email:Provider</c> is <c>Smtp</c>.</summary>
public class SmtpEmailSender : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Smtp.Host))
        {
            throw new InvalidOperationException("Email:Smtp:Host must be configured when Provider is Smtp.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };
        message.To.Add(to);

        using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = _options.Smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
        {
            client.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        _logger.LogInformation("Sending SMTP email to {To} with subject {Subject}", to, subject);
        await client.SendMailAsync(message, cancellationToken);
    }
}
