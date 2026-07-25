using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Application.Features.Reference.CreateFacility;
using FacilityInspection.Application.Features.Reference.CreateLocation;
using FacilityInspection.Application.Features.Reference.GetFacilities;
using FacilityInspection.Application.Features.Reference.GetLocations;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>
/// Reference/lookup data for the frontend (facilities, locations and enum dropdowns).
/// All endpoints require authentication; creating facilities/locations is Admin-only.
/// GET endpoints return active records only.
/// </summary>
[ApiController]
[Route("api/reference")]
[Authorize]
[Produces("application/json")]
public class ReferenceController : ControllerBase
{
    private readonly ISender _mediator;

    public ReferenceController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ---- Facilities ----

    /// <summary>Lists active facilities.</summary>
    [HttpGet("facilities")]
    [ProducesResponseType(typeof(IReadOnlyList<FacilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FacilityResponse>>> GetFacilities(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetActiveFacilitiesQuery(), cancellationToken));

    /// <summary>Creates a facility.</summary>
    [HttpPost("facilities")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FacilityResponse>> CreateFacility(
        [FromBody] CreateFacilityCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/reference/facilities/{response.Id}", response);
    }

    // ---- Locations ----

    /// <summary>Lists active locations, optionally filtered by facility.</summary>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(IReadOnlyList<LocationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocationResponse>>> GetLocations(
        [FromQuery] Guid? facilityId = null,
        CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetActiveLocationsQuery(facilityId), cancellationToken));

    /// <summary>Creates a location.</summary>
    [HttpPost("locations")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationResponse>> CreateLocation(
        [FromBody] CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Created($"/api/reference/locations/{response.Id}", response);
    }

    // ---- Enum dropdowns (static) ----

    /// <summary>Inspection classification options.</summary>
    [HttpGet("inspection-classifications")]
    [ProducesResponseType(typeof(IReadOnlyList<EnumOptionResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GetInspectionClassifications()
        => Ok(ToOptions<InspectionClassification>());

    /// <summary>Vendor category options.</summary>
    [HttpGet("vendor-categories")]
    [ProducesResponseType(typeof(IReadOnlyList<EnumOptionResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GetVendorCategories()
        => Ok(ToOptions<VendorCategory>());

    /// <summary>Incident status options.</summary>
    [HttpGet("incident-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<EnumOptionResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GetIncidentStatuses()
        => Ok(ToOptions<IncidentStatus>());

    /// <summary>Work-order status options.</summary>
    [HttpGet("work-order-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<EnumOptionResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GetWorkOrderStatuses()
        => Ok(ToOptions<WorkOrderStatus>());

    /// <summary>Verification decision options.</summary>
    [HttpGet("verification-decisions")]
    [ProducesResponseType(typeof(IReadOnlyList<EnumOptionResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GetVerificationDecisions()
        => Ok(ToOptions<VerificationDecision>());

    private static List<EnumOptionResponse> ToOptions<TEnum>() where TEnum : struct, Enum
        => Enum.GetValues<TEnum>()
            .Select(value => new EnumOptionResponse
            {
                Id = Convert.ToInt32(value),
                Name = value.ToString(),
            })
            .ToList();
}
