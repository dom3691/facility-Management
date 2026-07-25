using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// A progress update posted by a vendor against a work order. Forms an append-only
/// history of the remediation work.
/// </summary>
public class VendorUpdate : AuditableEntity, ISoftDelete
{
    public Guid WorkOrderId { get; set; }

    /// <summary>Logical reference to the vendor user posting the update (AspNetUsers.Id).</summary>
    public Guid UpdatedByUserId { get; set; }

    public DateTimeOffset UpdateDate { get; set; }

    /// <summary>Work-order status reported at the time of this update.</summary>
    public WorkOrderStatus StatusAtUpdate { get; set; }

    /// <summary>Free-text progress comment (optional on a completion update).</summary>
    public string? Notes { get; set; }

    /// <summary>Optional completion percentage (0–100).</summary>
    public int? PercentComplete { get; set; }

    /// <summary>True when this update marks the work order complete.</summary>
    public bool IsCompletionUpdate { get; set; }

    /// <summary>Completion narrative; required on a completion update.</summary>
    public string? CompletionComment { get; set; }

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;

    public ICollection<VendorUpdateAttachment> Attachments { get; set; } = new List<VendorUpdateAttachment>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
