using FluentValidation;

namespace FacilityInspection.Application.Features.WorkOrders.GenerateWorkOrder;

public class GenerateWorkOrderCommandValidator : AbstractValidator<GenerateWorkOrderCommand>
{
    public GenerateWorkOrderCommandValidator()
    {
        RuleFor(x => x.VendorAssignmentId)
            .NotEmpty().WithMessage("VendorAssignmentId is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}
