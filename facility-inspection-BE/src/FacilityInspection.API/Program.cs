using Asp.Versioning.ApiExplorer;
using FacilityInspection.API.Extensions;
using FacilityInspection.API.Middleware;
using FacilityInspection.Application;
using FacilityInspection.Infrastructure;
using FacilityInspection.Persistence;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Text.Json.Serialization;

// Bootstrap logger — active before the host is built so startup failures are captured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Facility Inspection API host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
    // --- Logging: replace the default providers with Serilog, read from configuration. ---
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // --- Register services layer by layer (dependencies flow inward). ---
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // --- HTTP request pipeline (order is significant). ---

    // 1) Correlation id (outermost) so every log line + response carries it.
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2) Request logging wraps the exception boundary so it logs the final status code.
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 3) Exception boundary converts thrown exceptions into the ApiResponse error envelope.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }

    app.UseHttpsRedirection();

    app.UseCors(FacilityInspection.API.Extensions.DependencyInjection.AngularCorsPolicy);

    app.UseAuthentication();
    app.UseAuthorization();

    // Background job dashboard (default authorization allows local requests only).
    app.UseHangfireDashboard("/hangfire");

    app.MapControllers();

    // --- Health check endpoints. ---
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Facility Inspection API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Exposed so integration tests can reference the entry point via WebApplicationFactory<Program>.
public partial class Program;
