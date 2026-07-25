using FacilityInspection.Application.Features.Incidents.CreateIncident;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class CreateIncidentCommandValidatorTests
{
    private readonly CreateIncidentCommandValidator _validator = new();

    private static CreateIncidentCommand ValidCommand() => new()
    {
        BusinessUnit = "Facilities",
        SAPId = "SAP-100234",
        FacilityId = Guid.NewGuid(),
        LocationId = Guid.NewGuid(),
        IncidentDate = DateTimeOffset.UtcNow,
        Description = "AC unit leaking in server room B.",
    };

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_mandatory_fields_fail_with_expected_errors()
    {
        var result = _validator.Validate(new CreateIncidentCommand());

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[]
        {
            nameof(CreateIncidentCommand.BusinessUnit),
            nameof(CreateIncidentCommand.SAPId),
            nameof(CreateIncidentCommand.FacilityId),
            nameof(CreateIncidentCommand.LocationId),
            nameof(CreateIncidentCommand.IncidentDate),
            nameof(CreateIncidentCommand.Description),
        });
    }
}
