using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A file (photo, document) captured during an inspection.
/// </summary>
public class InspectionAttachment : AuditableEntity, ISoftDelete
{
    public Guid InspectionId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    // Navigation
    public Inspection Inspection { get; set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
