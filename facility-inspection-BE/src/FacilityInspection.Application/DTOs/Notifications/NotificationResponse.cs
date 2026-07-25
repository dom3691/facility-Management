namespace FacilityInspection.Application.DTOs.Notifications;

/// <summary>Notification detail returned by the notification endpoints.</summary>
public record NotificationResponse
{
    public Guid Id { get; init; }

    public Guid RecipientUserId { get; init; }

    public string? RecipientEmail { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? RelatedEntityName { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public bool IsRead { get; init; }

    public DateTimeOffset CreatedDate { get; init; }

    public DateTimeOffset? SentDate { get; init; }

    public DateTimeOffset? ReadDate { get; init; }

    public string? ErrorMessage { get; init; }
}
