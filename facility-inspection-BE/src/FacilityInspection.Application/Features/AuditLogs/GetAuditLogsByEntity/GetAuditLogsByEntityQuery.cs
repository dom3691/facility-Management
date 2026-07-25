using FacilityInspection.Application.DTOs.AuditLogs;
using MediatR;

namespace FacilityInspection.Application.Features.AuditLogs.GetAuditLogsByEntity;

/// <summary>Returns the audit trail for a specific entity instance.</summary>
public record GetAuditLogsByEntityQuery(string EntityName, string EntityId)
    : IRequest<IReadOnlyList<AuditLogResponse>>;
