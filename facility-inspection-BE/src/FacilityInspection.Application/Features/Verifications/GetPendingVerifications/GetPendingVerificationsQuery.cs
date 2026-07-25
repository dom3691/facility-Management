using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetPendingVerifications;

/// <summary>
/// Returns work orders awaiting verification (Completed). Admin/Inspector see all; an
/// Initiator sees only work orders for the incidents they reported.
/// </summary>
public record GetPendingVerificationsQuery : IRequest<PaginatedResult<WorkOrderResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
