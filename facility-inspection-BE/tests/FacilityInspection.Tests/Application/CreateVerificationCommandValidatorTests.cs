using FacilityInspection.Application.Features.Verifications.CreateVerification;
using FacilityInspection.Domain.Enums;
using FluentAssertions;

namespace FacilityInspection.Tests.Application;

public class CreateVerificationCommandValidatorTests
{
    private readonly CreateVerificationCommandValidator _validator = new();

    [Fact]
    public void Fixed_without_comments_passes()
    {
        var result = _validator.Validate(new CreateVerificationCommand
        {
            WorkOrderId = Guid.NewGuid(),
            Decision = VerificationDecision.Fixed,
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NotFixed_without_comments_fails()
    {
        var result = _validator.Validate(new CreateVerificationCommand
        {
            WorkOrderId = Guid.NewGuid(),
            Decision = VerificationDecision.NotFixed,
            Comments = null,
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName)
            .Should().Contain(nameof(CreateVerificationCommand.Comments));
    }

    [Fact]
    public void NotFixed_with_comments_passes()
    {
        var result = _validator.Validate(new CreateVerificationCommand
        {
            WorkOrderId = Guid.NewGuid(),
            Decision = VerificationDecision.NotFixed,
            Comments = "Leak still present near the valve.",
        });

        result.IsValid.Should().BeTrue();
    }
}
