using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.Verifications.CreateVerification;
using FacilityInspection.Application.Features.Verifications.GetPendingVerifications;
using FacilityInspection.Application.Features.Verifications.GetVerificationById;
using FacilityInspection.Application.Features.Verifications.GetVerificationsByWorkOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Verification and closure of completed work orders.</summary>
[ApiController]
[Route("api/verifications")]
[Authorize]
[Produces("application/json")]
public class VerificationsController : ControllerBase
{
    private const string CanCreate = AppRoles.Initiator + "," + AppRoles.Admin;
    private const string CanViewPending = AppRoles.Initiator + "," + AppRoles.Inspector + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public VerificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Records a Fixed / NotFixed verification on a completed work order.</summary>
    [HttpPost]
    [Authorize(Roles = CanCreate)]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationResponse>> Create(
        [FromBody] CreateVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVerificationCommand
        {
            WorkOrderId = request.WorkOrderId,
            Decision = request.Decision,
            Comments = request.Comments,
        };

        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Gets a verification by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVerificationByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets all verifications for a work order.</summary>
    [HttpGet("by-work-order/{workOrderId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<VerificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<VerificationResponse>>> GetByWorkOrder(
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVerificationsByWorkOrderQuery(workOrderId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists work orders awaiting verification, paged.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = CanViewPending)]
    [ProducesResponseType(typeof(PaginatedResult<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WorkOrderResponse>>> GetPending(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPendingVerificationsQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }
}
