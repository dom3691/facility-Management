using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// The unit of work issued to a vendor for a specific assignment. Progress is
/// tracked via vendor updates and the outcome is confirmed by a verification.
/// </summary>
public class WorkOrder : AuditableEntity, ISoftDelete
{
    /// <summary>Human-friendly, unique reference, format <c>WO-yyyy-000001</c>.</summary>
    public string WorkOrderNumber { get; set; } = string.Empty;

    public Guid IncidentId { get; set; }

    public Guid VendorAssignmentId { get; set; }

    public Guid VendorId { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Assigned;

    public string? Description { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    // Navigation
    public Incident Incident { get; set; } = null!;

    public Vendor Vendor { get; set; } = null!;

    public VendorAssignment VendorAssignment { get; set; } = null!;

    public ICollection<VendorUpdate> Updates { get; set; } = new List<VendorUpdate>();

    /// <summary>Verification history (a NotFixed decision can lead to re-verification after rework).</summary>
    public ICollection<Verification> Verifications { get; set; } = new List<Verification>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
