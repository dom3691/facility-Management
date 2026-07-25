using FluentValidation;

namespace FacilityInspection.Application.Features.Reference.CreateFacility;

public class CreateFacilityCommandValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);
    }
}
