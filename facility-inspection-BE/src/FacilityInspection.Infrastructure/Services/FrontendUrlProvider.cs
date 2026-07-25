using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FacilityInspection.Infrastructure.Services;

public class FrontendUrlProvider : IFrontendUrlProvider
{
    private readonly FrontendOptions _options;

    public FrontendUrlProvider(IOptions<FrontendOptions> options)
    {
        _options = options.Value;
    }

    public string GetSetPasswordUrl(string email, string token)
        => BuildUrl("/auth/reset-password", email, token);

    public string GetResetPasswordUrl(string email, string token)
        => BuildUrl("/auth/reset-password", email, token);

    private string BuildUrl(string path, string email, string token)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        return $"{baseUrl}{path}?{query}";
    }
}
