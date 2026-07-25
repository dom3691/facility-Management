using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetPendingVerifications;

public class GetPendingVerificationsQueryHandler
    : IRequestHandler<GetPendingVerificationsQuery, PaginatedResult<WorkOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPendingVerificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<WorkOrderResponse>> Handle(
        GetPendingVerificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        // Admin/Inspector see all pending; an Initiator is scoped to their own incidents.
        Guid? initiatorFilter = null;
        if (!_currentUser.Roles.Contains(AppRoles.Admin) && !_currentUser.Roles.Contains(AppRoles.Inspector))
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
            {
                throw new ForbiddenAccessException("The current user could not be resolved.");
            }

            initiatorFilter = userId;
        }

        var (items, totalCount) = await _unitOfWork.WorkOrders.GetPendingVerificationPagedAsync(
            pageNumber, pageSize, initiatorFilter, cancellationToken);

        var mapped = items.Select(w => w.ToResponse()).ToList();
        return PaginatedResult<WorkOrderResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
