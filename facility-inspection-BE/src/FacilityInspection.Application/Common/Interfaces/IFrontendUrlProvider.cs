namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>Frontend URLs used when composing account emails.</summary>
public interface IFrontendUrlProvider
{
    string GetSetPasswordUrl(string email, string token);

    string GetResetPasswordUrl(string email, string token);
}
