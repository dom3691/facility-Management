using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Application.Features.Verifications.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Verifications.GetVerificationsByWorkOrder;

public class GetVerificationsByWorkOrderQueryHandler
    : IRequestHandler<GetVerificationsByWorkOrderQuery, IReadOnlyList<VerificationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetVerificationsByWorkOrderQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VerificationResponse>> Handle(
        GetVerificationsByWorkOrderQuery request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdWithDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        await VerificationAuthorization.EnsureCanViewAsync(
            workOrder, _currentUser, _identityService, cancellationToken);

        var verifications = await _unitOfWork.Verifications.GetByWorkOrderIdAsync(request.WorkOrderId, cancellationToken);
        return verifications.Select(v => v.ToResponse()).ToList();
    }
}
