using System.Net;
using System.Text.Json;
using FacilityInspection.API.Common;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Domain.Exceptions;
using ValidationException = FacilityInspection.Application.Common.Exceptions.ValidationException;

namespace FacilityInspection.API.Middleware;

/// <summary>
/// Terminal exception boundary for the HTTP pipeline. Converts known application/domain
/// exceptions into the uniform <see cref="ApiResponse{T}"/> error envelope, and shields
/// unexpected errors behind a generic 500 while logging the detail server-side.
/// </summary>
public class ExceptionHandlingMiddleware
{
    // Keep `data` present (as null) on errors; per-property [JsonIgnore] handles optional fields.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = Map(exception);

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType} on {Method} {Path}: {Message}",
                exception.GetType().Name, context.Request.Method, context.Request.Path, exception.Message);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started; the error envelope could not be written.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse.Failure(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private (HttpStatusCode StatusCode, string Message, IReadOnlyList<ErrorDetails> Errors) Map(Exception exception)
        => exception switch
        {
            ValidationException validation => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                validation.Errors
                    .SelectMany(kvp => kvp.Value.Select(m => new ErrorDetails(kvp.Key, m)))
                    .ToList()),

            BadRequestException => (HttpStatusCode.BadRequest, exception.Message, Single(exception.Message)),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, Single(exception.Message)),
            DomainException => (HttpStatusCode.BadRequest, exception.Message, Single(exception.Message)),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, exception.Message, Single(exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message, Single(exception.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                _environment.IsProduction() ? Array.Empty<ErrorDetails>() : Single(exception.ToString())),
        };

    private static IReadOnlyList<ErrorDetails> Single(string message)
        => new[] { new ErrorDetails(message) };
}
