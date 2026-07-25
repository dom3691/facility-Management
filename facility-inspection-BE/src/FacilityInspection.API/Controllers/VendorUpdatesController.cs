using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.VendorUpdates;
using FacilityInspection.Application.Features.VendorUpdates.CreateVendorUpdate;
using FacilityInspection.Application.Features.VendorUpdates.GetVendorUpdatesByWorkOrder;
using FacilityInspection.Application.Features.VendorUpdates.MarkWorkOrderComplete;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Vendor progress updates and completion.</summary>
[ApiController]
[Route("api/vendor-updates")]
[Authorize(Roles = Roles)]
[Produces("application/json")]
public class VendorUpdatesController : ControllerBase
{
    private const string Roles = AppRoles.Vendor + "," + AppRoles.Admin;

    private readonly ISender _mediator;

    public VendorUpdatesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Records a vendor progress update (with optional evidence).</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VendorUpdateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorUpdateResponse>> Create(
        [FromForm] CreateVendorUpdateRequest request,
        List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        var command = new CreateVendorUpdateCommand
        {
            WorkOrderId = request.WorkOrderId,
            ProgressComment = request.ProgressComment,
            ProgressPercentage = request.ProgressPercentage,
            Attachments = await ToUploadsAsync(attachments, cancellationToken),
        };

        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByWorkOrder), new { workOrderId = response.WorkOrderId }, response);
    }

    /// <summary>Gets all progress updates for a work order.</summary>
    [HttpGet("by-work-order/{workOrderId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorUpdateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<VendorUpdateResponse>>> GetByWorkOrder(
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVendorUpdatesByWorkOrderQuery(workOrderId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Marks a work order complete (with completion comment and evidence).</summary>
    [HttpPost("{workOrderId:guid}/mark-complete")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VendorUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorUpdateResponse>> MarkComplete(
        Guid workOrderId,
        [FromForm] MarkWorkOrderCompleteRequest request,
        List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        var command = new MarkWorkOrderCompleteCommand
        {
            WorkOrderId = workOrderId,
            CompletionComment = request.CompletionComment,
            Attachments = await ToUploadsAsync(attachments, cancellationToken),
        };

        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
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
