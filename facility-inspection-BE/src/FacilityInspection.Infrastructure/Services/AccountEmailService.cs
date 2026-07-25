using FacilityInspection.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Infrastructure.Services;

/// <summary>
/// Sends account emails via <see cref="IEmailService"/>. Templates are plain-text for MVP;
/// swap the body builder when HTML templates are introduced.
/// </summary>
public class AccountEmailService : IAccountEmailService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountEmailService> _logger;

    public AccountEmailService(IEmailService emailService, ILogger<AccountEmailService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(
        string to,
        string fullName,
        string setPasswordUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = "You're invited to Facility Inspection";
        var body = $"""
            Hello {fullName},

            An administrator has created an account for you in the Facility Inspection system.

            Please set your password using the link below (valid for 72 hours):

            {setPasswordUrl}

            After setting your password you can sign in with your email address.

            If you did not expect this email, you can ignore it.

            — Facility Inspection System
            """;

        _logger.LogInformation("Sending invitation email to {Email}", to);
        return _emailService.SendEmailAsync(to, subject, body, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(
        string to,
        string fullName,
        string resetPasswordUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset your Facility Inspection password";
        var body = $"""
            Hello {fullName},

            We received a request to reset your password. Use the link below (valid for 72 hours):

            {resetPasswordUrl}

            If you did not request a reset, you can ignore this email.

            — Facility Inspection System
            """;

        _logger.LogInformation("Sending password reset email to {Email}", to);
        return _emailService.SendEmailAsync(to, subject, body, cancellationToken);
    }
}
