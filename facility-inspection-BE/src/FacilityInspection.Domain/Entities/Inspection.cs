using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// An inspector's assessment of an incident. The resulting
/// <see cref="Classification"/> determines whether the incident is closed
/// (Good) or routed to vendor assignment for remediation.
/// </summary>
public class Inspection : AuditableEntity, ISoftDelete
{
    public Guid IncidentId { get; set; }

    /// <summary>Logical reference to the inspecting user (AspNetUsers.Id).</summary>
    public Guid InspectorUserId { get; set; }

    public DateTimeOffset InspectionDate { get; set; }

    public InspectionClassification Classification { get; set; }

    public string Findings { get; set; } = string.Empty;

    public string? Recommendation { get; set; }

    /// <summary>True when the classification requires vendor remediation.</summary>
    public bool RequiresVendor { get; set; }

    // Navigation
    public Incident Incident { get; set; } = null!;

    public ICollection<InspectionAttachment> Attachments { get; set; } = new List<InspectionAttachment>();

    public ICollection<VendorAssignment> VendorAssignments { get; set; } = new List<VendorAssignment>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
