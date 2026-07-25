using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorUpdates;
using MediatR;

namespace FacilityInspection.Application.Features.VendorUpdates.CreateVendorUpdate;

/// <summary>Records a vendor progress update against a work order (with optional evidence).</summary>
public record CreateVendorUpdateCommand : IRequest<VendorUpdateResponse>
{
    public Guid WorkOrderId { get; init; }

    public string ProgressComment { get; init; } = string.Empty;

    public int? ProgressPercentage { get; init; }

    public IReadOnlyList<FileUpload> Attachments { get; init; } = Array.Empty<FileUpload>();
}
