namespace FacilityInspection.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed JWT configuration bound from the "JwtSettings" configuration section.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; init; } = 60;
}
