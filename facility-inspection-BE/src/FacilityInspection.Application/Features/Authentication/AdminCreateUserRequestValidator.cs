using FacilityInspection.Application.Common.Authorization;
using FluentValidation;

namespace FacilityInspection.Application.Features.Authentication;

public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequest>
{
    public AdminCreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.SAPId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AppRoles.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}.");

        RuleFor(x => x.VendorId)
            .NotEmpty()
            .When(x => x.Role == AppRoles.Vendor)
            .WithMessage("VendorId is required when creating a Vendor user.");
    }
}
