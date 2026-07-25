using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.AuditLogs;
using MediatR;

namespace FacilityInspection.Application.Features.AuditLogs.GetAuditLogs;

/// <summary>Returns a page of audit logs (admin), optionally filtered by entity name / action.</summary>
public record GetAuditLogsQuery : IRequest<PaginatedResult<AuditLogResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? EntityName { get; init; }

    public string? Action { get; init; }
}
