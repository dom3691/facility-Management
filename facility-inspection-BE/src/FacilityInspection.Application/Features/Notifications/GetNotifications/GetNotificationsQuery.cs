using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Notifications;
using MediatR;

namespace FacilityInspection.Application.Features.Notifications.GetNotifications;

/// <summary>Returns a page of all notifications (admin), optionally filtered by read state.</summary>
public record GetNotificationsQuery : IRequest<PaginatedResult<NotificationResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public bool? IsRead { get; init; }
}
