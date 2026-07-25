using FacilityInspection.Application.Features.VendorUpdates.CreateVendorUpdate;
using FacilityInspection.Application.Features.VendorUpdates.MarkWorkOrderComplete;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class VendorUpdateValidatorTests
{
    [Fact]
    public void CreateVendorUpdate_valid_passes()
    {
        var result = new CreateVendorUpdateCommandValidator().Validate(new CreateVendorUpdateCommand
        {
            WorkOrderId = Guid.NewGuid(),
            ProgressComment = "Removed faulty compressor.",
            ProgressPercentage = 50,
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateVendorUpdate_missing_comment_and_bad_percentage_fail()
    {
        var result = new CreateVendorUpdateCommandValidator().Validate(new CreateVendorUpdateCommand
        {
            WorkOrderId = Guid.Empty,
            ProgressComment = string.Empty,
            ProgressPercentage = 150,
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[]
        {
            nameof(CreateVendorUpdateCommand.WorkOrderId),
            nameof(CreateVendorUpdateCommand.ProgressComment),
            nameof(CreateVendorUpdateCommand.ProgressPercentage),
        });
    }

    [Fact]
    public void MarkComplete_requires_completion_comment()
    {
        var result = new MarkWorkOrderCompleteCommandValidator().Validate(new MarkWorkOrderCompleteCommand
        {
            WorkOrderId = Guid.NewGuid(),
            CompletionComment = string.Empty,
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName)
            .Should().Contain(nameof(MarkWorkOrderCompleteCommand.CompletionComment));
    }
}
