using FluentValidation.Results;

namespace FacilityInspection.Application.Common.Exceptions;

/// <summary>
/// Aggregates FluentValidation failures into a single exception that the API
/// middleware maps to an HTTP 400 with a field-keyed error dictionary.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    /// <summary>
    /// Creates a validation exception from a pre-built error dictionary. Lets non-FluentValidation
    /// sources (e.g. ASP.NET Core Identity results) surface errors through the same HTTP 400 shape.
    /// </summary>
    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
