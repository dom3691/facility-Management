using FacilityInspection.Application.DTOs.WorkOrders;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrdersByIncident;

/// <summary>Returns all work orders for an incident.</summary>
public record GetWorkOrdersByIncidentQuery(Guid IncidentId) : IRequest<IReadOnlyList<WorkOrderResponse>>;
