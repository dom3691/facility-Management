using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.UpdateWorkOrderStatus;

/// <summary>Advances a work order's status. <see cref="Id"/> comes from the route.</summary>
public record UpdateWorkOrderStatusCommand : IRequest<WorkOrderResponse>
{
    public Guid Id { get; init; }

    public WorkOrderStatus Status { get; init; }
}
