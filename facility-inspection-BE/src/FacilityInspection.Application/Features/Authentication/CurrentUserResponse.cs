namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Result of <c>GET /api/auth/me</c> — the authenticated user's profile and roles.</summary>
public record CurrentUserResponse
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string? SAPId { get; init; }

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    public bool RequiresPasswordChange { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
