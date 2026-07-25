using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrderById;

public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetWorkOrderByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<WorkOrderResponse> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // GetByIdWithDetailsAsync eager-loads Incident + Vendor, which the ownership check needs.
        var workOrder = await _unitOfWork.WorkOrders.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        // Object-level check: a vendor may only see their own work orders; an initiator only
        // those on their own incidents; Admin/Inspector see all.
        await WorkOrderAuthorization.EnsureCanViewAsync(
            workOrder, _currentUser, _identityService, cancellationToken);

        return workOrder.ToResponse();
    }
}
