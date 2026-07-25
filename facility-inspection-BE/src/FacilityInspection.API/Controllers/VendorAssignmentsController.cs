using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Application.Features.VendorAssignments.CreateVendorAssignment;
using FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentById;
using FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentByIncident;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Vendor assignment (routing an inspected incident to a vendor).</summary>
[ApiController]
[Route("api/vendor-assignments")]
[Authorize]
[Produces("application/json")]
public class VendorAssignmentsController : ControllerBase
{
    private const string CanAssign = AppRoles.Inspector + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public VendorAssignmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Assigns a vendor to an inspected incident.</summary>
    [HttpPost]
    [Authorize(Roles = CanAssign)]
    [ProducesResponseType(typeof(VendorAssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorAssignmentResponse>> Create(
        [FromBody] CreateVendorAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Gets a vendor assignment by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendorAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorAssignmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVendorAssignmentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets all vendor assignments for an incident.</summary>
    [HttpGet("by-incident/{incidentId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorAssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VendorAssignmentResponse>>> GetByIncident(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVendorAssignmentByIncidentQuery(incidentId), cancellationToken);
        return Ok(result);
    }
}
