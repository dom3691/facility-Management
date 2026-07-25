using FluentValidation;

namespace FacilityInspection.Application.Features.VendorUpdates.CreateVendorUpdate;

public class CreateVendorUpdateCommandValidator : AbstractValidator<CreateVendorUpdateCommand>
{
    private const int MaxAttachments = 10;

    public CreateVendorUpdateCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("WorkOrderId is required.");

        RuleFor(x => x.ProgressComment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.ProgressPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.ProgressPercentage.HasValue);

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
