using FluentValidation;

namespace FacilityInspection.Application.Features.WorkOrders.UpdateWorkOrderStatus;

public class UpdateWorkOrderStatusCommandValidator : AbstractValidator<UpdateWorkOrderStatusCommand>
{
    public UpdateWorkOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid work-order status.");
    }
}
