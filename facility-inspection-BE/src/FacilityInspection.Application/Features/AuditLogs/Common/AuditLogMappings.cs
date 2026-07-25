using FacilityInspection.Application.DTOs.AuditLogs;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.AuditLogs.Common;

/// <summary>Manual entity → DTO mapping for audit logs.</summary>
public static class AuditLogMappings
{
    public static AuditLogResponse ToResponse(this AuditLog auditLog) => new()
    {
        Id = auditLog.Id,
        EntityName = auditLog.EntityName,
        EntityId = auditLog.EntityId,
        Action = auditLog.Action,
        OldValues = auditLog.OldValues,
        NewValues = auditLog.NewValues,
        PerformedByUserId = auditLog.PerformedByUserId,
        PerformedByName = auditLog.PerformedByName,
        PerformedDate = auditLog.PerformedDate,
        IpAddress = auditLog.IpAddress,
        UserAgent = auditLog.UserAgent,
    };
}
