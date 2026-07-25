namespace FacilityInspection.Application.Common.Models;

/// <summary>
/// A generated access token and its absolute UTC expiry, returned by
/// <see cref="Interfaces.IJwtTokenGenerator"/>.
/// </summary>
public record JwtToken(string AccessToken, DateTime ExpiresAtUtc);
