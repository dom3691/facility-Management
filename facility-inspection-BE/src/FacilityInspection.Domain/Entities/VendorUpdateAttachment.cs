using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A file (photo, document) attached to a vendor's progress update — typically
/// evidence of work performed.
/// </summary>
public class VendorUpdateAttachment : AuditableEntity, ISoftDelete
{
    public Guid VendorUpdateId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    // Navigation
    public VendorUpdate VendorUpdate { get; set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
