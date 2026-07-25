using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorUpdates;
using MediatR;

namespace FacilityInspection.Application.Features.VendorUpdates.MarkWorkOrderComplete;

/// <summary>Marks a work order complete. <see cref="WorkOrderId"/> comes from the route.</summary>
public record MarkWorkOrderCompleteCommand : IRequest<VendorUpdateResponse>
{
    public Guid WorkOrderId { get; init; }

    public string CompletionComment { get; init; } = string.Empty;

    public IReadOnlyList<FileUpload> Attachments { get; init; } = Array.Empty<FileUpload>();
}
