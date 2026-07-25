using System.Security.Claims;
using FacilityInspection.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FacilityInspection.Infrastructure.Identity;

/// <summary>
/// Resolves the caller's identity from the current ASP.NET Core HTTP context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("sub");

    public string? UserName => Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua
            ? ua
            : null;
}
