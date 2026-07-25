using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// An in-app/outbound notification addressed to a user, usually raised by a
/// workflow event. Can point back to the originating entity via
/// <see cref="RelatedEntityName"/> / <see cref="RelatedEntityId"/>.
/// <para>
/// <see cref="Status"/> tracks the send lifecycle (Pending → Sent/Failed);
/// <see cref="IsRead"/> tracks whether the recipient has read it.
/// </para>
/// </summary>
public class Notification : AuditableEntity
{
    /// <summary>Logical reference to the recipient user (AspNetUsers.Id).</summary>
    public Guid RecipientUserId { get; set; }

    /// <summary>Recipient email, resolved when the notification is dispatched.</summary>
    public string? RecipientEmail { get; set; }

    public NotificationType Type { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>Name of the entity that triggered this notification (e.g. "Incident").</summary>
    public string? RelatedEntityName { get; set; }

    /// <summary>Identifier of the entity that triggered this notification.</summary>
    public Guid? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? SentDate { get; set; }

    public DateTimeOffset? ReadDate { get; set; }

    /// <summary>Populated when a dispatch attempt fails.</summary>
    public string? ErrorMessage { get; set; }
}
