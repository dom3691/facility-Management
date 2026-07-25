namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Outbound email abstraction. Implemented in Infrastructure and invoked by the notification
/// dispatch job. A logging implementation ships with the MVP.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
