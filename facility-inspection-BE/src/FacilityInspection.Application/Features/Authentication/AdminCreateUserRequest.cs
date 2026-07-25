namespace FacilityInspection.Application.Features.Authentication;

/// <summary>Payload for Admin-only <c>POST /api/auth/users</c>. Password is auto-generated server-side.</summary>
public class AdminCreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string SAPId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    /// <summary>Required when <see cref="Role"/> is <c>Vendor</c>; links the login to a vendor company.</summary>
    public Guid? VendorId { get; set; }
}
