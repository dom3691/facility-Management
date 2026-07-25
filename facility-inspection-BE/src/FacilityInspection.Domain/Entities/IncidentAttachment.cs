using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A file (photo, document) attached to an incident report.
/// </summary>
public class IncidentAttachment : AuditableEntity, ISoftDelete
{
    public Guid IncidentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>Storage location / URL of the persisted file (blob path, disk path, etc.).</summary>
    public string StoragePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    // Navigation
    public Incident Incident { get; set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
