using FluentValidation;

namespace FacilityInspection.Application.Features.VendorAssignments.CreateVendorAssignment;

public class CreateVendorAssignmentCommandValidator : AbstractValidator<CreateVendorAssignmentCommand>
{
    public CreateVendorAssignmentCommandValidator()
    {
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.InspectionId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();

        RuleFor(x => x.VendorCategory)
            .IsInEnum().WithMessage("VendorCategory must be one of: InHouse, Leadway, ExternalVendor.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
