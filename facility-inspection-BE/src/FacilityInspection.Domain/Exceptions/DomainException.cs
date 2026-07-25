namespace FacilityInspection.Domain.Exceptions;

/// <summary>
/// Raised when a domain invariant or business rule is violated.
/// Translated to an HTTP 4xx response by the API exception middleware.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
