using FluentValidation;

namespace FacilityInspection.Application.Features.Vendors.UpdateVendor;

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.VendorName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.VendorCategory)
            .IsInEnum().WithMessage("VendorCategory must be one of: InHouse, Leadway, ExternalVendor.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50);
    }
}
