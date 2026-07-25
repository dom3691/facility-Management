using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrders;

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, PaginatedResult<WorkOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWorkOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<WorkOrderResponse>> Handle(
        GetWorkOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.WorkOrders.GetPagedAsync(
            pageNumber, pageSize, request.Status, vendorId: null, cancellationToken);

        var mapped = items.Select(w => w.ToResponse()).ToList();
        return PaginatedResult<WorkOrderResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
