namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Provides the identity of the caller for the current request.
/// Implemented in Infrastructure using the ASP.NET Core HTTP context.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }

    /// <summary>The caller's IP address for the current request, if available.</summary>
    string? IpAddress { get; }

    /// <summary>The caller's User-Agent for the current request, if available.</summary>
    string? UserAgent { get; }
}
