using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A service provider that can be assigned to remediate incidents. Master data.
/// </summary>
public class Vendor : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;

    public VendorCategory Category { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<VendorAssignment> VendorAssignments { get; set; } = new List<VendorAssignment>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
