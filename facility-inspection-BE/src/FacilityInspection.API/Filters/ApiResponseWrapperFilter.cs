using FacilityInspection.API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FacilityInspection.API.Filters;

/// <summary>
/// Wraps successful controller results in the uniform <see cref="ApiResponse{T}"/> envelope,
/// so every action returns <c>{ success, message, data, errors }</c> without per-action boilerplate.
/// Error responses are produced by <see cref="Middleware.ExceptionHandlingMiddleware"/>.
/// </summary>
public class ApiResponseWrapperFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Mutate the ObjectResult in place so its status code and headers (e.g. Location on 201)
        // are preserved.
        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

            if (statusCode is >= 200 and < 300 && !IsAlreadyWrapped(objectResult.Value))
            {
                objectResult.Value = ApiResponse.Success(objectResult.Value);
                objectResult.DeclaredType = typeof(ApiResponse<object?>);
            }
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool IsAlreadyWrapped(object? value)
        => value is not null
           && value.GetType() is { IsGenericType: true } type
           && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
}
