namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Result of Admin user provisioning. No access token is issued to the caller.</summary>
public record CreateUserResponse
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>True when the invitation/set-password email was queued for delivery.</summary>
    public bool InvitationEmailSent { get; init; }
}
