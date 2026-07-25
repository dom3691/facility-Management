namespace FacilityInspection.Application.Common.Exceptions;

/// <summary>
/// Thrown for a malformed or semantically invalid request that isn't a field-level
/// validation failure. Mapped to HTTP 400 by the API exception middleware.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message)
    {
    }
}
