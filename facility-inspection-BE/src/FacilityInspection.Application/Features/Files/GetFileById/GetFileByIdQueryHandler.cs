using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Files;
using FacilityInspection.Application.Features.Files.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Files.GetFileById;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, FileMetadataResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetFileByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<FileMetadataResponse> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var info = await FileAccessResolver.ResolveAsync(
            request.FileId, _unitOfWork, _currentUser, _identityService, cancellationToken);

        return new FileMetadataResponse
        {
            Id = info.Id,
            FileName = info.FileName,
            ContentType = info.ContentType,
            FileSizeBytes = info.FileSizeBytes,
            Module = info.Module,
        };
    }
}
