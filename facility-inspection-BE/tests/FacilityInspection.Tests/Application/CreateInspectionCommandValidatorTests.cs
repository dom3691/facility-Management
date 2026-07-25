using FacilityInspection.Application.Features.Inspections.CreateInspection;
using FacilityInspection.Domain.Enums;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class CreateInspectionCommandValidatorTests
{
    private readonly CreateInspectionCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new CreateInspectionCommand
        {
            IncidentId = Guid.NewGuid(),
            Classification = InspectionClassification.Faulty,
            Comments = "Compressor failed; needs replacement.",
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_incident_and_comments_fail()
    {
        var result = _validator.Validate(new CreateInspectionCommand
        {
            IncidentId = Guid.Empty,
            Classification = InspectionClassification.Good,
            Comments = string.Empty,
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[]
        {
            nameof(CreateInspectionCommand.IncidentId),
            nameof(CreateInspectionCommand.Comments),
        });
    }

    [Fact]
    public void Out_of_range_classification_fails()
    {
        var result = _validator.Validate(new CreateInspectionCommand
        {
            IncidentId = Guid.NewGuid(),
            Classification = (InspectionClassification)999,
            Comments = "Some comment.",
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName)
            .Should().Contain(nameof(CreateInspectionCommand.Classification));
    }
}
