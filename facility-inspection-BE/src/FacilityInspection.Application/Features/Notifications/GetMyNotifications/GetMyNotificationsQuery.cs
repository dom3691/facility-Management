using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Notifications;
using MediatR;

namespace FacilityInspection.Application.Features.Notifications.GetMyNotifications;

/// <summary>Returns a page of the current user's notifications.</summary>
public record GetMyNotificationsQuery : IRequest<PaginatedResult<NotificationResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public bool? IsRead { get; init; }
}
