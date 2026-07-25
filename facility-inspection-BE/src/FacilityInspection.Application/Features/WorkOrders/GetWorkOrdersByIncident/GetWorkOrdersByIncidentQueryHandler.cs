using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Application.Features.WorkOrders.Common;
using MediatR;

namespace FacilityInspection.Application.Features.WorkOrders.GetWorkOrdersByIncident;

public class GetWorkOrdersByIncidentQueryHandler
    : IRequestHandler<GetWorkOrdersByIncidentQuery, IReadOnlyList<WorkOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWorkOrdersByIncidentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkOrderResponse>> Handle(
        GetWorkOrdersByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var workOrders = await _unitOfWork.WorkOrders.GetByIncidentIdAsync(request.IncidentId, cancellationToken);
        return workOrders.Select(w => w.ToResponse()).ToList();
    }
}
