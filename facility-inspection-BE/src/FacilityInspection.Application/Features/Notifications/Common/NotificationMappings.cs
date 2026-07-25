using FacilityInspection.Application.DTOs.Notifications;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Notifications.Common;

/// <summary>Manual entity → DTO mapping for notifications.</summary>
public static class NotificationMappings
{
    public static NotificationResponse ToResponse(this Notification notification) => new()
    {
        Id = notification.Id,
        RecipientUserId = notification.RecipientUserId,
        RecipientEmail = notification.RecipientEmail,
        Type = notification.Type.ToString(),
        Status = notification.Status.ToString(),
        Title = notification.Title,
        Message = notification.Message,
        RelatedEntityName = notification.RelatedEntityName,
        RelatedEntityId = notification.RelatedEntityId,
        IsRead = notification.IsRead,
        CreatedDate = notification.CreatedDate,
        SentDate = notification.SentDate,
        ReadDate = notification.ReadDate,
        ErrorMessage = notification.ErrorMessage,
    };
}
