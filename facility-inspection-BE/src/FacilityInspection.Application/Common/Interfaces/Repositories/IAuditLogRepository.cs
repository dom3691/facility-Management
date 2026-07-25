using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Common.Interfaces.Repositories;

/// <summary>Read/append repository for the <see cref="AuditLog"/> trail (never deleted).</summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Returns a page of audit logs (newest first), optionally filtered by entity name and
    /// action, with the total count.
    /// </summary>
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? entityName,
        string? action,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the audit trail for a specific entity instance (newest first).</summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default);
}
