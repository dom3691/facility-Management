using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.CreateIncident;
using FacilityInspection.Application.Features.Incidents.GetIncidentById;
using FacilityInspection.Application.Features.Incidents.GetIncidents;
using FacilityInspection.Application.Features.Incidents.GetMyIncidents;
using FacilityInspection.Application.Features.Incidents.GetPendingInspectionIncidents;
using FacilityInspection.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Incident logging and querying (Sprint 1 MVP).</summary>
[ApiController]
[Route("api/incidents")]
[Authorize]
[Produces("application/json")]
public class IncidentsController : ControllerBase
{
    private const string CanCreate = AppRoles.Initiator + "," + AppRoles.Admin;
    private const string CanViewQueue = AppRoles.Inspector + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public IncidentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Logs a new incident (with optional photo/document attachments).</summary>
    [HttpPost]
    [Authorize(Roles = CanCreate)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentResponse>> Create(
        [FromForm] CreateIncidentRequest request,
        List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        var command = new CreateIncidentCommand
        {
            BusinessUnit = request.BusinessUnit,
            SAPId = request.SAPId,
            FacilityId = request.FacilityId,
            LocationId = request.LocationId,
            IncidentDate = request.IncidentDate,
            Description = request.Description,
            Attachments = await ToUploadsAsync(attachments, cancellationToken),
        };  

        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Lists ALL incidents (optionally filtered by status), paged. Operational queue view —
    /// restricted to Admin/Inspector. Initiators use <c>GET /api/incidents/my</c> for their own.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = CanViewQueue)]
    [ProducesResponseType(typeof(PaginatedResult<IncidentListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<IncidentListResponse>>> GetIncidents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] IncidentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetIncidentsQuery { PageNumber = pageNumber, PageSize = pageSize, Status = status },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single incident by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIncidentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists incidents reported by the current user, paged.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PaginatedResult<IncidentListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<IncidentListResponse>>> GetMyIncidents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyIncidentsQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>The inspection queue: incidents pending inspection, paged.</summary>
    [HttpGet("pending-inspection")]
    [Authorize(Roles = CanViewQueue)]
    [ProducesResponseType(typeof(PaginatedResult<IncidentListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<IncidentListResponse>>> GetPendingInspection(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPendingInspectionIncidentsQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    private static async Task<List<FileUpload>> ToUploadsAsync(
        IEnumerable<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        var uploads = new List<FileUpload>();
        if (files is null)
        {
            return uploads;
        }

        foreach (var file in files.Where(f => f.Length > 0))
        {
            using var memory = new MemoryStream();
            await file.CopyToAsync(memory, cancellationToken);
            uploads.Add(new FileUpload(
                file.FileName,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                memory.ToArray()));
        }

        return uploads;
    }
}
