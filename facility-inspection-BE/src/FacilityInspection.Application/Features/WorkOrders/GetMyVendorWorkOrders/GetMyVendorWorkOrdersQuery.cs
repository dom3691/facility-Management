using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetMyVendorWorkOrders;

/// <summary>Returns a page of work orders assigned to the current user's vendor.</summary>
public record GetMyVendorWorkOrdersQuery : IRequest<PaginatedResult<WorkOrderResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
