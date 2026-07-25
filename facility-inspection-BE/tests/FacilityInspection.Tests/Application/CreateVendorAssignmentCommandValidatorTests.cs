using FacilityInspection.Application.Features.VendorAssignments.CreateVendorAssignment;
using FacilityInspection.Domain.Enums;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class CreateVendorAssignmentCommandValidatorTests
{
    private readonly CreateVendorAssignmentCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new CreateVendorAssignmentCommand
        {
            IncidentId = Guid.NewGuid(),
            InspectionId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            VendorCategory = VendorCategory.ExternalVendor,
            Notes = "Priority repair.",
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_ids_and_bad_category_fail()
    {
        var result = _validator.Validate(new CreateVendorAssignmentCommand
        {
            IncidentId = Guid.Empty,
            InspectionId = Guid.Empty,
            VendorId = Guid.Empty,
            VendorCategory = (VendorCategory)0,
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[]
        {
            nameof(CreateVendorAssignmentCommand.IncidentId),
            nameof(CreateVendorAssignmentCommand.InspectionId),
            nameof(CreateVendorAssignmentCommand.VendorId),
            nameof(CreateVendorAssignmentCommand.VendorCategory),
        });
    }
}
