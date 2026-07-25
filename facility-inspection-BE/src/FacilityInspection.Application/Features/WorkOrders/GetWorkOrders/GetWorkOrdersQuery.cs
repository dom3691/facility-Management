using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrders;

/// <summary>Returns a page of work orders, optionally filtered by status.</summary>
public record GetWorkOrdersQuery : IRequest<PaginatedResult<WorkOrderResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public WorkOrderStatus? Status { get; init; }
}
