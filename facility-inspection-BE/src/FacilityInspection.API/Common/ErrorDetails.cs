using System.Text.Json.Serialization;

namespace FacilityInspection.API.Common;

/// <summary>A single error entry in an <see cref="ApiResponse{T}"/>.</summary>
public class ErrorDetails
{
    public ErrorDetails()
    {
    }

    public ErrorDetails(string message)
        => Message = message;

    public ErrorDetails(string? field, string message)
    {
        Field = field;
        Message = message;
    }

    /// <summary>The offending field/property, when the error is field-specific (validation).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; init; }

    public string Message { get; init; } = string.Empty;
}
