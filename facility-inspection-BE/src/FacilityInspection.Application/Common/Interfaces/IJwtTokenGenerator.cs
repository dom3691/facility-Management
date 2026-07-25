using FacilityInspection.Application.Common.Models;

namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Issues signed JWT access tokens. Implemented in Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Builds a signed access token carrying the user's id, email, full name, SAP id and roles.
    /// </summary>
    JwtToken GenerateToken(
        Guid userId,
        string email,
        string fullName,
        string? sapId,
        IEnumerable<string> roles);
}
