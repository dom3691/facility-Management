using FluentValidation;

namespace FacilityInspection.Application.Features.Inspections.CreateInspection;

public class CreateInspectionCommandValidator : AbstractValidator<CreateInspectionCommand>
{
    private const int MaxAttachments = 10;

    public CreateInspectionCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("IncidentId is required.");

        RuleFor(x => x.Classification)
            .IsInEnum().WithMessage("Classification must be one of: Good, Faulty, RunDown, Damaged.");

        RuleFor(x => x.Comments)
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
