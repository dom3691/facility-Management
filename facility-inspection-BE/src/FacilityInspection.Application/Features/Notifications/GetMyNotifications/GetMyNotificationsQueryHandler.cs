using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Notifications;
using FacilityInspection.Application.Features.Notifications.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Notifications.GetMyNotifications;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, PaginatedResult<NotificationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<NotificationResponse>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Notifications.GetPagedAsync(
            pageNumber, pageSize, recipientUserId: userId, request.IsRead, cancellationToken);

        var mapped = items.Select(n => n.ToResponse()).ToList();
        return PaginatedResult<NotificationResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
