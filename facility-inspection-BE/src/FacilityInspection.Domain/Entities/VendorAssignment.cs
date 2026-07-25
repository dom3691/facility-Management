using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// Links an inspected incident that requires remediation to the vendor selected to perform
/// the work. A single work order is issued per assignment.
/// </summary>
public class VendorAssignment : AuditableEntity, ISoftDelete
{
    public Guid IncidentId { get; set; }

    public Guid InspectionId { get; set; }

    public Guid VendorId { get; set; }

    /// <summary>Category selected for the assignment (must match the vendor's category).</summary>
    public VendorCategory VendorCategory { get; set; }

    /// <summary>Logical reference to the user who made the assignment (AspNetUsers.Id).</summary>
    public Guid AssignedByUserId { get; set; }

    public DateTimeOffset AssignedDate { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public Incident Incident { get; set; } = null!;

    public Inspection Inspection { get; set; } = null!;

    public Vendor Vendor { get; set; } = null!;

    public WorkOrder? WorkOrder { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
