using Serilog.Context;

namespace FacilityInspection.API.Middleware;

/// <summary>
/// Assigns a correlation id to every request: echoes an inbound <c>X-Correlation-ID</c> header
/// or generates one, exposes it on <c>HttpContext.Items</c> and the response header, and pushes
/// it to the Serilog log context so every log line for the request carries it.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var provided)
                            && !string.IsNullOrWhiteSpace(provided)
            ? provided.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty(ItemKey, correlationId))
        {
            await _next(context);
        }
    }
}
