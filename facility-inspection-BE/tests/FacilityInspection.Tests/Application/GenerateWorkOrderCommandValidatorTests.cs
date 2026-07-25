using FacilityInspection.Application.Features.WorkOrders.GenerateWorkOrder;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class GenerateWorkOrderCommandValidatorTests
{
    private readonly GenerateWorkOrderCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new GenerateWorkOrderCommand
        {
            VendorAssignmentId = Guid.NewGuid(),
            Description = "Replace compressor unit.",
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_vendor_assignment_fails()
    {
        var result = _validator.Validate(new GenerateWorkOrderCommand { VendorAssignmentId = Guid.Empty });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName)
            .Should().Contain(nameof(GenerateWorkOrderCommand.VendorAssignmentId));
    }
}
