namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Result of a successful register/login: the access token plus identity summary.</summary>
public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public string TokenType { get; init; } = "Bearer";

    public DateTime ExpiresAtUtc { get; init; }

    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string? SAPId { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>When true the client must prompt for a password change before using the app.</summary>
    public bool RequiresPasswordChange { get; init; }
}
