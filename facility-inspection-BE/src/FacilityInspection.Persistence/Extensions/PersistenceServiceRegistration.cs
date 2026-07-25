using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Persistence.Context;
using FacilityInspection.Persistence.Identity;
using FacilityInspection.Persistence.Auditing;
using FacilityInspection.Persistence.Interceptors;
using FacilityInspection.Persistence.Notifications;
using FacilityInspection.Persistence.Repositories;
using FacilityInspection.Persistence.Seed;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacilityInspection.Persistence;

/// <summary>
/// Composition root for the Persistence layer. Wires EF Core (SQL Server), the auditing
/// interceptor, ASP.NET Core Identity stores, the <see cref="IApplicationDbContext"/>
/// abstraction, and the repository / unit-of-work services.
/// </summary>
public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(72));

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();

        // Seeds the canonical roles at startup (idempotent, non-fatal).
        services.AddHostedService<DatabaseSeederHostedService>();

        AddRepositories(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Generic repository for any entity type.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Typed repositories for the main aggregates.
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IVendorAssignmentRepository, VendorAssignmentRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IVendorUpdateRepository, VendorUpdateRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Unit of work coordinating the repositories over one DbContext.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
