using FacilityInspection.Domain.Common;

namespace FacilityInspection.Domain.Entities;

/// <summary>
/// An immutable record of a significant action in the system. Deliberately extends
/// only <see cref="BaseEntity"/> — audit logs are never modified or soft-deleted and
/// carry their own actor/timestamp fields.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Name of the affected entity type (e.g. "WorkOrder").</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Identifier of the affected entity (string to allow any key shape).</summary>
    public string? EntityId { get; set; }

    /// <summary>The action performed (e.g. "Create", "Vendor Assigned", "Verification Fixed").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Serialized before-state (JSON), where applicable.</summary>
    public string? OldValues { get; set; }

    /// <summary>Serialized after-state (JSON), where applicable.</summary>
    public string? NewValues { get; set; }

    /// <summary>Logical reference to the acting user (AspNetUsers.Id); null for system actions.</summary>
    public Guid? PerformedByUserId { get; set; }

    public string? PerformedByName { get; set; }

    public DateTimeOffset PerformedDate { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}
