using FacilityInspection.Application.DTOs.WorkOrders;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrderById;

/// <summary>Returns a single work order, or 404 if it does not exist.</summary>
public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderResponse>;
