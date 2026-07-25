using FacilityInspection.Application.DTOs.Files;
using MediatR;

namespace FacilityInspection.Application.Features.Files.GetFileById;

/// <summary>Returns metadata for an attachment the caller is authorised to view.</summary>
public record GetFileByIdQuery(Guid FileId) : IRequest<FileMetadataResponse>;
