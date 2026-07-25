using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Notifications;
using FacilityInspection.Application.Features.Notifications.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Notifications.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, PaginatedResult<NotificationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<NotificationResponse>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Notifications.GetPagedAsync(
            pageNumber, pageSize, recipientUserId: null, request.IsRead, cancellationToken);

        var mapped = items.Select(n => n.ToResponse()).ToList();
        return PaginatedResult<NotificationResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
