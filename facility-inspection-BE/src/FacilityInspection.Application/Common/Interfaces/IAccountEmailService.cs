namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>Account-related outbound emails (invitations, password reset).</summary>
public interface IAccountEmailService
{
    Task SendInvitationEmailAsync(
        string to,
        string fullName,
        string setPasswordUrl,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string to,
        string fullName,
        string resetPasswordUrl,
        CancellationToken cancellationToken = default);
}
