using System.Diagnostics;

namespace FacilityInspection.API.Middleware;

/// <summary>
/// Logs one structured line per HTTP request (method, path, status, elapsed). The correlation
/// id is attached automatically via the Serilog log context. Noisy infra paths are skipped.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private static bool ShouldSkip(PathString path)
        => path.StartsWithSegments("/health")
           || path.StartsWithSegments("/hangfire")
           || path.StartsWithSegments("/swagger");
}
