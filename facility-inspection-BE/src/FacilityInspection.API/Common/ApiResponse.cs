namespace FacilityInspection.API.Common;

/// <summary>
/// The uniform envelope returned by every API endpoint:
/// <c>{ success, message, data, errors }</c>.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IReadOnlyList<ErrorDetails> Errors { get; init; } = Array.Empty<ErrorDetails>();

    public static ApiResponse<T> Ok(T? data, string message = "Request successful")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ErrorDetails> errors)
        => new() { Success = false, Message = message, Errors = errors };
}

/// <summary>Non-generic helpers for building envelopes around loosely-typed payloads.</summary>
public static class ApiResponse
{
    public static ApiResponse<object?> Success(object? data, string message = "Request successful")
        => ApiResponse<object?>.Ok(data, message);

    public static ApiResponse<object?> Failure(string message, IReadOnlyList<ErrorDetails> errors)
        => ApiResponse<object?>.Fail(message, errors);
}
