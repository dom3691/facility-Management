using FacilityInspection.Application;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Features.Incidents.CreateIncident;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = FacilityInspection.Application.Common.Exceptions.ValidationException;

namespace FacilityInspection.Tests.Application;

/// <summary>
/// Proves the CQRS/MediatR + FluentValidation pipeline is wired for the Incident feature:
/// the validation behaviour rejects an invalid command before the handler runs.
/// </summary>
public class CreateIncidentPipelineTests
{
    private static ISender BuildSender()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        // Handler dependencies (normally Infrastructure/Persistence) — mocked so the
        // provider resolves; validation short-circuits before they are used.
        services.AddScoped(_ => new Mock<IUnitOfWork>().Object);
        services.AddScoped(_ => new Mock<IFileStorageService>().Object);
        services.AddScoped(_ => new Mock<IIdentityService>().Object);
        services.AddScoped<ICurrentUserService, StubCurrentUser>();
        services.AddSingleton<IDateTimeProvider, StubClock>();

        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Invalid_command_is_rejected_by_validation_behaviour()
    {
        var sender = BuildSender();

        var act = async () => await sender.Send(new CreateIncidentCommand
        {
            BusinessUnit = string.Empty, // all mandatory fields missing
            SAPId = string.Empty,
            Description = string.Empty,
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        public string? UserId => "11111111-1111-1111-1111-111111111111";
        public string? UserName => "tester";
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles => new[] { "Initiator" };
        public string? IpAddress => "127.0.0.1";
        public string? UserAgent => "xunit";
    }

    private sealed class StubClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
