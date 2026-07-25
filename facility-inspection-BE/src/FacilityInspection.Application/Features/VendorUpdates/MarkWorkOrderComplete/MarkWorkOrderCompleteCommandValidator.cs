using FluentValidation;

namespace FacilityInspection.Application.Features.VendorUpdates.MarkWorkOrderComplete;

public class MarkWorkOrderCompleteCommandValidator : AbstractValidator<MarkWorkOrderCompleteCommand>
{
    private const int MaxAttachments = 10;

    public MarkWorkOrderCompleteCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty();

        RuleFor(x => x.CompletionComment)
            .NotEmpty().WithMessage("CompletionComment is required.")
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
