using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.GenerateWorkOrder;
using FacilityInspection.Application.Features.WorkOrders.GetMyVendorWorkOrders;
using FacilityInspection.Application.Features.WorkOrders.GetWorkOrderById;
using FacilityInspection.Application.Features.WorkOrders.GetWorkOrders;
using FacilityInspection.Application.Features.WorkOrders.GetWorkOrdersByIncident;
using FacilityInspection.Application.Features.WorkOrders.UpdateWorkOrderStatus;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Work-order generation, querying and status transitions.</summary>
[ApiController]
[Route("api/work-orders")]
[Authorize]
[Produces("application/json")]
public class WorkOrdersController : ControllerBase
{
    private const string CanGenerate = AppRoles.Inspector + "," + AppRoles.Admin;
    private const string CanViewAll = AppRoles.Admin + "," + AppRoles.Inspector;
    private const string CanUpdateStatus = AppRoles.Vendor + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public WorkOrdersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Generates a work order for a completed vendor assignment.</summary>
    [HttpPost("generate")]
    [Authorize(Roles = CanGenerate)]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> Generate(
        [FromBody] GenerateWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Lists work orders (optionally filtered by status), paged.</summary>
    [HttpGet]
    [Authorize(Roles = CanViewAll)]
    [ProducesResponseType(typeof(PaginatedResult<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WorkOrderResponse>>> GetWorkOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] WorkOrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetWorkOrdersQuery { PageNumber = pageNumber, PageSize = pageSize, Status = status },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a work order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWorkOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders for an incident. Cross-vendor enumeration, so restricted to
    /// Admin/Inspector. A vendor sees only their own via <c>my-vendor-work-orders</c>.
    /// </summary>
    [HttpGet("by-incident/{incidentId:guid}")]
    [Authorize(Roles = CanViewAll)]
    [ProducesResponseType(typeof(IReadOnlyList<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkOrderResponse>>> GetByIncident(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWorkOrdersByIncidentQuery(incidentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists work orders assigned to the current user's vendor, paged.</summary>
    [HttpGet("my-vendor-work-orders")]
    [ProducesResponseType(typeof(PaginatedResult<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WorkOrderResponse>>> GetMyVendorWorkOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyVendorWorkOrdersQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Advances a work order's status (Assigned → InProgress → Completed).</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = CanUpdateStatus)]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateWorkOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }
}
