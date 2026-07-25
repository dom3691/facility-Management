using System.Text;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Infrastructure.Authentication;
using FacilityInspection.Infrastructure.BackgroundJobs;
using FacilityInspection.Infrastructure.Configuration;
using FacilityInspection.Infrastructure.FileStorage;
using FacilityInspection.Infrastructure.Identity;
using FacilityInspection.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FacilityInspection.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Registers technical services:
/// current-user accessor, clock, email sender, JWT authentication + token generation,
/// and Hangfire background processing.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAccountEmailService, AccountEmailService>();
        services.AddSingleton<IFrontendUrlProvider, FrontendUrlProvider>();

        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        var emailProvider = configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>()?.Provider
            ?? "Logging";
        if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailService, LoggingEmailSender>();
        }

        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        AddJwtAuthentication(services, configuration);
        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? new JwtOptions();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
                                ? new string('*', 32) // placeholder so startup validation doesn't crash pre-config
                                : jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnection = configuration.GetConnectionString("HangfireConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConnection, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
            }));

        services.AddHangfireServer();

        // Notification dispatch job + the recurring-job registrar.
        services.AddScoped<NotificationBackgroundJob>();
        services.AddHostedService<RecurringJobsHostedService>();
    }
}
