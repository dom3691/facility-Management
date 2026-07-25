using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FacilityInspection.Infrastructure.Authentication;

/// <summary>
/// Produces signed HS256 JWT access tokens from the configured <see cref="JwtOptions"/>.
/// Tokens carry the user id, email, full name, SAP id and role claims.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string FullNameClaimType = "fullName";
    public const string SapIdClaimType = "sapId";

    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTime;

    public JwtTokenGenerator(IOptions<JwtOptions> options, IDateTimeProvider dateTime)
    {
        _options = options.Value;
        _dateTime = dateTime;
    }

    public JwtToken GenerateToken(
        Guid userId,
        string email,
        string fullName,
        string? sapId,
        IEnumerable<string> roles)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, email),
            new(FullNameClaimType, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (!string.IsNullOrWhiteSpace(sapId))
        {
            claims.Add(new Claim(SapIdClaimType, sapId));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAtUtc = _dateTime.UtcNow.UtcDateTime.AddMinutes(_options.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtToken(accessToken, expiresAtUtc);
    }
}
