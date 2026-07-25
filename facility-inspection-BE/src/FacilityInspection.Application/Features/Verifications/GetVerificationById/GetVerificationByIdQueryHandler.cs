using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Application.Features.Verifications.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetVerificationById;

public class GetVerificationByIdQueryHandler
    : IRequestHandler<GetVerificationByIdQuery, VerificationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetVerificationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<VerificationResponse> Handle(GetVerificationByIdQuery request, CancellationToken cancellationToken)
    {
        var verification = await _unitOfWork.Verifications.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Verification), request.Id);

        await VerificationAuthorization.EnsureCanViewAsync(
            verification.WorkOrder, _currentUser, _identityService, cancellationToken);

        return verification.ToResponse();
    }
}
