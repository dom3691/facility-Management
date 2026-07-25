using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A specific location within a <see cref="Facility"/> (floor, wing, room, zone) where
/// an incident can occur or an asset resides. Reference/master data.
/// </summary>
public class Location : AuditableEntity, ISoftDelete
{
    public Guid FacilityId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Optional short code for the location within its facility.</summary>
    public string? Code { get; set; }

    /// <summary>Optional floor / area / zone descriptor.</summary>
    public string? FloorOrArea { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Facility Facility { get; set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
