using System.Text.Json;
using FacilityInspection.Application.Common.Audit;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Persistence.Auditing;

/// <summary>
/// Persistence-backed <see cref="IAuditService"/>. Serializes before/after state to JSON and
/// captures the acting user/IP/user-agent from <see cref="ICurrentUserService"/>. Entries
/// enlist in the shared unit of work (no save) so they commit with the audited action.
/// </summary>
public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public Task LogAsync(
        string entityName,
        string entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        Guid? performedByUserId = Guid.TryParse(_currentUser.UserId, out var userId) ? userId : null;

        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = Serialize(oldValues),
            NewValues = Serialize(newValues),
            PerformedByUserId = performedByUserId,
            PerformedByName = _currentUser.UserName,
            PerformedDate = _dateTime.UtcNow,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
        };

        // Enlist only — the caller's SaveChangesAsync commits it with the audited action.
        return _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public Task LogCreateAsync(
        string entityName,
        string entityId,
        object? newValues = null,
        CancellationToken cancellationToken = default)
        => LogAsync(entityName, entityId, AuditActions.Create, oldValues: null, newValues, cancellationToken);

    public Task LogUpdateAsync(
        string entityName,
        string entityId,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken = default)
        => LogAsync(entityName, entityId, AuditActions.Update, oldValues, newValues, cancellationToken);

    public Task LogDeleteAsync(
        string entityName,
        string entityId,
        object? oldValues = null,
        CancellationToken cancellationToken = default)
        => LogAsync(entityName, entityId, AuditActions.Delete, oldValues, newValues: null, cancellationToken);

    public Task LogWorkflowActionAsync(
        string entityName,
        string entityId,
        string action,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
        => LogAsync(entityName, entityId, action, oldValues, newValues, cancellationToken);

    private static string? Serialize(object? value)
        => value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
}
