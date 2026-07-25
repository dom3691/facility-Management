namespace FacilityInspection.Domain.Common;

/// <summary>
/// Base type for entities that track creation and modification metadata.
/// The audit fields are populated automatically by the persistence auditing interceptor.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
}
