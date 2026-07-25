using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetMyVendorWorkOrders;

public class GetMyVendorWorkOrdersQueryHandler
    : IRequestHandler<GetMyVendorWorkOrdersQuery, PaginatedResult<WorkOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetMyVendorWorkOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<WorkOrderResponse>> Handle(
        GetMyVendorWorkOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var vendorId = await _identityService.GetUserVendorIdAsync(userId, cancellationToken);
        if (vendorId is null)
        {
            // Not linked to a vendor — nothing to show.
            return PaginatedResult<WorkOrderResponse>.Create(
                Array.Empty<WorkOrderResponse>(), 0, pageNumber, pageSize);
        }

        var (items, totalCount) = await _unitOfWork.WorkOrders.GetPagedAsync(
            pageNumber, pageSize, status: null, vendorId: vendorId, cancellationToken);

        var mapped = items.Select(w => w.ToResponse()).ToList();
        return PaginatedResult<WorkOrderResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
