using System.Diagnostics;
using FacilityInspection.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that times each request and logs a warning when it
/// exceeds the slow-request threshold, so hotspots surface in the logs.
/// </summary>
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMs = 500;

    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public PerformanceBehaviour(
        ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        if (timer.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Long-running request {RequestName} ({ElapsedMilliseconds} ms) for user {UserId}",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds,
                _currentUser.UserId ?? "anonymous");
        }

        return response;
    }
}
