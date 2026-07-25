using FacilityInspection.Domain.Enums;
using FluentValidation;

namespace FacilityInspection.Application.Features.Verifications.CreateVerification;

public class CreateVerificationCommandValidator : AbstractValidator<CreateVerificationCommand>
{
    public CreateVerificationCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty();

        RuleFor(x => x.Decision)
            .IsInEnum().WithMessage("Decision must be either Fixed or NotFixed.");

        RuleFor(x => x.Comments)
            .MaximumLength(2000);

        // Comments are mandatory when the work is rejected.
        RuleFor(x => x.Comments)
            .NotEmpty().WithMessage("Comments are required when the decision is NotFixed.")
            .When(x => x.Decision == VerificationDecision.NotFixed);
    }
}
