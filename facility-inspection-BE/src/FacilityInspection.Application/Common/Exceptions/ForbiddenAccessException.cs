namespace FacilityInspection.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated user lacks permission for an operation.
/// Mapped to HTTP 403 by the API exception middleware.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You do not have permission to perform this action.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
