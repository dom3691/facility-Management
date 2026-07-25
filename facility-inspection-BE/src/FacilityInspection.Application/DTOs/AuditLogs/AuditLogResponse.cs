namespace FacilityInspection.Application.DTOs.AuditLogs;

/// <summary>Audit-trail record returned by the audit endpoints.</summary>
public record AuditLogResponse
{
    public Guid Id { get; init; }

    public string EntityName { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string? OldValues { get; init; }

    public string? NewValues { get; init; }

    public Guid? PerformedByUserId { get; init; }

    public string? PerformedByName { get; init; }

    public DateTimeOffset PerformedDate { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }
}
