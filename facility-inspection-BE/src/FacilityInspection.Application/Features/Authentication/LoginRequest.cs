namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Payload for <c>POST /api/auth/login</c>.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
