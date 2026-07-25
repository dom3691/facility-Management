namespace FacilityInspection.Domain.Common;

/// <summary>
/// Marks an entity that is never physically removed. The persistence layer converts
/// deletes into a flag update and applies a global query filter so soft-deleted rows
/// are excluded from normal queries.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    string? DeletedBy { get; set; }

    DateTimeOffset? DeletedDate { get; set; }
}
