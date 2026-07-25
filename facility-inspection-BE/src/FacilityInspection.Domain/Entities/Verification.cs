using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// The final sign-off on a completed work order. A <see cref="VerificationDecision.Fixed"/>
/// decision closes the incident; <see cref="VerificationDecision.NotFixed"/> reopens remediation.
/// </summary>
public class Verification : AuditableEntity, ISoftDelete
{
    public Guid WorkOrderId { get; set; }

    /// <summary>Logical reference to the verifying user (AspNetUsers.Id).</summary>
    public Guid VerifiedByUserId { get; set; }

    public DateTimeOffset VerificationDate { get; set; }

    public VerificationDecision Decision { get; set; }

    public string? Remarks { get; set; }

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
