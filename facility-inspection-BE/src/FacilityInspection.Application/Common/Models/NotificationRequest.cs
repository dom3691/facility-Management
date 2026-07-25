using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.Common.Models;

/// <summary>
/// The data needed to queue a single notification. Passed to
/// <see cref="Interfaces.INotificationService.QueueNotificationAsync"/> by workflow handlers.
/// </summary>
public record NotificationRequest
{
    public Guid RecipientUserId { get; init; }

    public NotificationType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? RelatedEntityName { get; init; }

    public Guid? RelatedEntityId { get; init; }
}
