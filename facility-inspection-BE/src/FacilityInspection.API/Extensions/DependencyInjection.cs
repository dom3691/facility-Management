using Asp.Versioning;
using FacilityInspection.API.Filters;
using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Persistence.Context;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FacilityInspection.API.Extensions;

/// <summary>
/// Composition root for the API (presentation) layer: controllers, API versioning,
/// Swagger/OpenAPI, CORS for the Angular client and health checks.
/// </summary>
public static class DependencyInjection
{
    public const string AngularCorsPolicy = "AngularClient";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<ApiResponseWrapperFilter>());
        services.AddEndpointsApiExplorer();

        services.AddApiVersioningConfigured();
        services.AddSwaggerConfigured();
        services.AddCorsConfigured(configuration);
        services.AddHealthChecksConfigured(configuration);
        services.AddAuthorizationPolicies();

        return services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.RequireAdmin, p => p.RequireRole(AppRoles.Admin));
            options.AddPolicy(AuthPolicies.RequireInitiator, p => p.RequireRole(AppRoles.Initiator));
            options.AddPolicy(AuthPolicies.RequireInspector, p => p.RequireRole(AppRoles.Inspector));
            options.AddPolicy(AuthPolicies.RequireVendor, p => p.RequireRole(AppRoles.Vendor));
        });

        return services;
    }

    private static IServiceCollection AddApiVersioningConfigured(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    private static IServiceCollection AddSwaggerConfigured(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        return services;
    }

    private static IServiceCollection AddCorsConfigured(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:4200" };

        services.AddCors(options =>
        {
            options.AddPolicy(AngularCorsPolicy, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return services;
    }

    private static IServiceCollection AddHealthChecksConfigured(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var builder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.AddSqlServer(
                connectionString,
                name: "sql-server",
                tags: new[] { "db", "ready" });
        }

        builder.AddDbContextCheck<ApplicationDbContext>(
            name: "application-dbcontext",
            tags: new[] { "db", "ready" });

        return services;
    }
}
