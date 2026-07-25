using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Application.Features.Vendors.CreateVendor;
using FacilityInspection.Application.Features.Vendors.DeleteVendor;
using FacilityInspection.Application.Features.Vendors.GetVendorById;
using FacilityInspection.Application.Features.Vendors.GetVendors;
using FacilityInspection.Application.Features.Vendors.UpdateVendor;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Vendor master-data management.</summary>
[ApiController]
[Route("api/vendors")]
[Authorize]
[Produces("application/json")]
public class VendorsController : ControllerBase
{
    private const string CanManage = AppRoles.Admin;
    private const string CanView = AppRoles.Inspector + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public VendorsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a vendor.</summary>
    [HttpPost]
    [Authorize(Roles = CanManage)]
    [ProducesResponseType(typeof(VendorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorResponse>> Create(
        [FromBody] CreateVendorCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Lists vendors (optionally filtered by category / active), paged.</summary>
    [HttpGet]
    [Authorize(Roles = CanView)]
    [ProducesResponseType(typeof(PaginatedResult<VendorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<VendorResponse>>> GetVendors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] VendorCategory? category = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetVendorsQuery { PageNumber = pageNumber, PageSize = pageSize, Category = category, IsActive = isActive },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a vendor by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = CanView)]
    [ProducesResponseType(typeof(VendorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVendorByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates a vendor.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = CanManage)]
    [ProducesResponseType(typeof(VendorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorResponse>> Update(
        Guid id,
        [FromBody] UpdateVendorCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Soft-deletes a vendor.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = CanManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteVendorCommand(id), cancellationToken);
        return NoContent();
    }
}
