namespace FacilityInspection.Application.Common.Authorization;

/// <summary>
/// Named authorization policy identifiers. Each maps to a single role requirement
/// (see the API's policy registration).
/// </summary>
public static class AuthPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireInitiator = "RequireInitiator";
    public const string RequireInspector = "RequireInspector";
    public const string RequireVendor = "RequireVendor";
}
