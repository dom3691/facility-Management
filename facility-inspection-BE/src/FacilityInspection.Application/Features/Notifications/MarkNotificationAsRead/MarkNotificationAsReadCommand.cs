using MediatR;

namespace FacilityInspection.Application.Features.Notifications.MarkNotificationAsRead;

/// <summary>Marks one of the current user's notifications as read.</summary>
public record MarkNotificationAsReadCommand(Guid Id) : IRequest;
