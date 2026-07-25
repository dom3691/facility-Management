using FacilityInspection.Application.Features.Files.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Files.DownloadFile;

/// <summary>
/// Resolves and authorises an attachment, returning the info needed to stream it (including the
/// internal storage path — used server-side only, never serialized to the client).
/// </summary>
public record DownloadFileQuery(Guid FileId) : IRequest<FileAttachmentInfo>;
