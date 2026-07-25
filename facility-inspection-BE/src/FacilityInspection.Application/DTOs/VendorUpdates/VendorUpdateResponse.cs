using FacilityInspection.Application.DTOs.Common;

namespace FacilityInspection.Application.DTOs.VendorUpdates;

/// <summary>Vendor progress/completion update detail.</summary>
public record VendorUpdateResponse
{
    public Guid Id { get; init; }

    public Guid WorkOrderId { get; init; }

    public string? ProgressComment { get; init; }

    public int? ProgressPercentage { get; init; }

    public bool IsCompletionUpdate { get; init; }

    public string? CompletionComment { get; init; }

    public string StatusAtUpdate { get; init; } = string.Empty;

    public Guid UpdatedByUserId { get; init; }

    public DateTimeOffset UpdatedDate { get; init; }

    public IReadOnlyList<AttachmentResponse> Attachments { get; init; }
        = Array.Empty<AttachmentResponse>();
}
