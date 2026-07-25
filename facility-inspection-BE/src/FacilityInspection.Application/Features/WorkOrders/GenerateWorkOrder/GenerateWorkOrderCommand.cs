using FacilityInspection.Application.DTOs.WorkOrders;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GenerateWorkOrder;

/// <summary>
/// Generates a work order for a completed vendor assignment. Bound from the JSON body.
/// If <see cref="Description"/> is omitted, the incident's description is used.
/// </summary>
public record GenerateWorkOrderCommand : IRequest<WorkOrderResponse>
{
    public Guid VendorAssignmentId { get; init; }

    public string? Description { get; init; }
}
