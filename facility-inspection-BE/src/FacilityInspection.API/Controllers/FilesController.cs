using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.DTOs.Files;
using FacilityInspection.Application.Features.Files.DownloadFile;
using FacilityInspection.Application.Features.Files.GetFileById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Access to stored attachments. A caller may only access files linked to records
/// they are authorised to view; Admin can access all.</summary>
[ApiController]
[Route("api/files")]
[Authorize]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorage;

    public FilesController(ISender mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    /// <summary>Returns metadata for an attachment.</summary>
    [HttpGet("{fileId:guid}")]
    [ProducesResponseType(typeof(FileMetadataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileMetadataResponse>> GetFileMetadata(Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFileByIdQuery(fileId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Downloads the binary content of an attachment (original file name preserved).</summary>
    [HttpGet("download/{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        var info = await _mediator.Send(new DownloadFileQuery(fileId), cancellationToken);
        var stream = await _fileStorage.GetAsync(info.StoragePath, cancellationToken);

        // FileStreamResult is not an ObjectResult, so it bypasses the ApiResponse envelope filter.
        return File(stream, info.ContentType, info.FileName);
    }
}
