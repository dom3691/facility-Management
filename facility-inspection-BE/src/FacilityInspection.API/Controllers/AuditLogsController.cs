using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.AuditLogs;
using FacilityInspection.Application.Features.AuditLogs.GetAuditLogs;
using FacilityInspection.Application.Features.AuditLogs.GetAuditLogsByEntity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>
/// Read-only access to the audit trail. Admin only. Audit logs are append-only — there is
/// deliberately no create/update/delete endpoint.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = AppRoles.Admin)]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly ISender _mediator;

    public AuditLogsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lists audit logs (optionally filtered by entity name / action), paged.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AuditLogResponse>>> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAuditLogsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                EntityName = entityName,
                Action = action,
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns the audit trail for a specific entity instance.</summary>
    [HttpGet("by-entity/{entityName}/{entityId}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogResponse>>> GetByEntity(
        string entityName,
        string entityId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAuditLogsByEntityQuery(entityName, entityId), cancellationToken);
        return Ok(result);
    }
}
