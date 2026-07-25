using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs any unhandled exception escaping a handler,
/// then rethrows so the API exception middleware can shape the HTTP response.
/// </summary>
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "Unhandled exception for request {RequestName} {@Request}", requestName, request);
            throw;
        }
    }
}
