namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Payload for <c>POST /api/auth/register</c>.</summary>
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string SAPId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = string.Empty;

    /// <summary>Role to assign. Optional; defaults to <c>Initiator</c> when omitted.</summary>
    public string? Role { get; set; }
}
