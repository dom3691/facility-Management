using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A physical facility (building/site) that is subject to inspection. Reference/master data.
/// A facility contains many <see cref="Location"/>s.
/// </summary>
public class Facility : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Short unique code for the facility (e.g. "HQ", "WH-01").</summary>
    public string Code { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Location> Locations { get; set; } = new List<Location>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
