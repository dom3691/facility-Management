namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Writes entries to the audit trail. The acting user, IP and user-agent are captured
/// automatically from the current request. Entries enlist in the caller's unit of work so
/// they commit atomically with the action being audited. Implemented in Persistence.
/// </summary>
public interface IAuditService
{
    /// <summary>Core method: records an action with optional before/after state.</summary>
    Task LogAsync(
        string entityName,
        string entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);

    Task LogCreateAsync(
        string entityName,
        string entityId,
        object? newValues = null,
        CancellationToken cancellationToken = default);

    Task LogUpdateAsync(
        string entityName,
        string entityId,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken = default);

    Task LogDeleteAsync(
        string entityName,
        string entityId,
        object? oldValues = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records a named workflow action (e.g. "Vendor Assigned", "Verification Fixed").</summary>
    Task LogWorkflowActionAsync(
        string entityName,
        string entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}
