namespace FacilityInspection.Domain.Common;

/// <summary>
/// Marks an entity that tracks creation and modification metadata.
/// The persistence auditing interceptor stamps these fields automatically.
/// </summary>
public interface IAuditableEntity
{
    string? CreatedBy { get; set; }

    DateTimeOffset CreatedDate { get; set; }

    string? ModifiedBy { get; set; }

    DateTimeOffset? ModifiedDate { get; set; }
}
