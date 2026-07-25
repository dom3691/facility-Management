using FacilityInspection.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs the start and successful completion of each
/// request, tagged with the calling user for traceability.
/// </summary>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehaviour(
        ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
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
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId ?? "anonymous";

        _logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, userId);

        var response = await next();

        _logger.LogInformation("Handled {RequestName} for user {UserId}", requestName, userId);

        return response;
    }
}
