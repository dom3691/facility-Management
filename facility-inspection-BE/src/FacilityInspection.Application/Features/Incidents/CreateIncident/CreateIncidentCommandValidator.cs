using FluentValidation;

namespace FacilityInspection.Application.Features.Incidents.CreateIncident;

public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    private const int MaxAttachments = 10;

    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.BusinessUnit)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.SAPId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FacilityId)
            .NotEmpty().WithMessage("FacilityId is required.");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("LocationId is required.");

        RuleFor(x => x.IncidentDate)
            .NotEmpty().WithMessage("IncidentDate is required.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Attachments)
            .Must(a => a.Count <= MaxAttachments)
            .WithMessage($"A maximum of {MaxAttachments} attachments is allowed.");

        // Per-file type/size limits are enforced from configuration by the file storage service.
        RuleForEach(x => x.Attachments).ChildRules(a =>
        {
            a.RuleFor(f => f.FileName).NotEmpty();
        });
    }
}
