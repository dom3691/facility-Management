using FacilityInspection.Persistence.Context;
using FacilityInspection.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Persistence.Seed;

/// <summary>
/// Runs idempotent identity seeding (roles) once at application startup.
/// <para>
/// Deliberately does NOT apply migrations — schema changes stay under the developer's control.
/// Seeding failures (e.g. the database/tables not yet created) are logged, not fatal, so the
/// host still starts; roles are seeded on the next startup once migrations are applied.
/// </para>
/// </summary>
public class DatabaseSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseSeederHostedService> _logger;

    public DatabaseSeederHostedService(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        ILogger<DatabaseSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await ApplicationDbContextSeed.SeedRolesAsync(roleManager, cancellationToken);
            await ApplicationDbContextSeed.SeedReferenceDataAsync(context, cancellationToken);
            _logger.LogInformation("Role and reference-data seeding completed.");

            // Sample users carry well-known passwords, so they are seeded in Development only.
            if (_environment.IsDevelopment())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                await ApplicationDbContextSeed.SeedDevelopmentUsersAsync(userManager, context, cancellationToken);
                _logger.LogInformation("Development sample users seeded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Identity role seeding skipped — is the database migrated and reachable? " +
                "Roles will be seeded on the next startup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
