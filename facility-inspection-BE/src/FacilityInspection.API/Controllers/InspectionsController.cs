using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Application.Features.Inspections.CreateInspection;
using FacilityInspection.Application.Features.Inspections.GetInspectionById;
using FacilityInspection.Application.Features.Inspections.GetInspectionsByIncident;
using FacilityInspection.Application.Features.Inspections.GetMyInspections;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Inspection recording and querying (Sprint 1 MVP).</summary>
[ApiController]
[Route("api/inspections")]
[Authorize]
[Produces("application/json")]
public class InspectionsController : ControllerBase
{
    private const string CanCreate = AppRoles.Inspector + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public InspectionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Records an inspection for a pending incident (with optional photo evidence).</summary>
    [HttpPost]
    [Authorize(Roles = CanCreate)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(InspectionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionResponse>> Create(
        [FromForm] CreateInspectionRequest request,
        List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        var command = new CreateInspectionCommand
        {
            IncidentId = request.IncidentId,
            Classification = request.Classification,
            Comments = request.Comments,
            Attachments = await ToUploadsAsync(attachments, cancellationToken),
        };

        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Gets a single inspection by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InspectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInspectionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets all inspections recorded against an incident.</summary>
    [HttpGet("by-incident/{incidentId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<InspectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<InspectionResponse>>> GetByIncident(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInspectionsByIncidentQuery(incidentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists inspections performed by the current user, paged.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PaginatedResult<InspectionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<InspectionResponse>>> GetMyInspections(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyInspectionsQuery { PageNumber = pageNumber, PageSize = pageSize },
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
