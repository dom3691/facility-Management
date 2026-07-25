namespace FacilityInspection.Application.Common.Authorization;

/// <summary>
/// Canonical application role names. Shared by role seeding, JWT issuance,
/// authorization policies and validators to avoid magic strings.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Initiator = "Initiator";
    public const string Inspector = "Inspector";
    public const string Vendor = "Vendor";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        Initiator,
        Inspector,
        Vendor,
    };
}
