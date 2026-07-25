using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.VendorUpdates;
using FacilityInspection.Application.Features.VendorUpdates.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.VendorUpdates.GetVendorUpdatesByWorkOrder;

public class GetVendorUpdatesByWorkOrderQueryHandler
    : IRequestHandler<GetVendorUpdatesByWorkOrderQuery, IReadOnlyList<VendorUpdateResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetVendorUpdatesByWorkOrderQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VendorUpdateResponse>> Handle(
        GetVendorUpdatesByWorkOrderQuery request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // A vendor may only view updates for their own work orders; Admin sees all.
        await VendorUpdateAuthorization.EnsureCanAccessWorkOrderAsync(
            workOrder, _currentUser, _identityService, cancellationToken);

        var updates = await _unitOfWork.VendorUpdates.GetByWorkOrderIdAsync(request.WorkOrderId, cancellationToken);
        return updates.Select(u => u.ToResponse()).ToList();
    }
}
