using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Features.Files.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Files.DownloadFile;

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, FileAttachmentInfo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public DownloadFileQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public Task<FileAttachmentInfo> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        => FileAccessResolver.ResolveAsync(
            request.FileId, _unitOfWork, _currentUser, _identityService, cancellationToken);
}
