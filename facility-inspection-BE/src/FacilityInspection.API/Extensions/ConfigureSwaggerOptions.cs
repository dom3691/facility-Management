using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FacilityInspection.API.Extensions;

/// <summary>
/// Generates one Swagger document per discovered API version and registers a
/// JWT bearer security scheme so protected endpoints are testable from the UI.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Facility Inspection API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? "Facility Inspection Automation System API. This version is deprecated."
                    : "Facility Inspection Automation System API.",
            });
        }

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter a valid JWT bearer token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            },
        };

        options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() },
        });
    }

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
}
