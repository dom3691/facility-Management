using System.Reflection;
using FacilityInspection.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FacilityInspection.Application;

/// <summary>
/// Composition root for the Application layer. Registers MediatR handlers,
/// FluentValidation validators and the cross-cutting request pipeline.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Registration order = outer→inner wrapping around the handler:
            //   UnhandledException → Logging → Validation → Performance → Handler
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
