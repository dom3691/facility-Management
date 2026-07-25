using FacilityInspection.Application.DTOs.VendorUpdates;
using MediatR;

namespace FacilityInspection.Application.Features.VendorUpdates.GetVendorUpdatesByWorkOrder;

/// <summary>Returns all progress updates for a work order.</summary>
public record GetVendorUpdatesByWorkOrderQuery(Guid WorkOrderId)
    : IRequest<IReadOnlyList<VendorUpdateResponse>>;
