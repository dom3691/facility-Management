using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Application.Features.VendorAssignments.Common;
using MediatR;

namespace FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentByIncident;

public class GetVendorAssignmentByIncidentQueryHandler
    : IRequestHandler<GetVendorAssignmentByIncidentQuery, IReadOnlyList<VendorAssignmentResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorAssignmentByIncidentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VendorAssignmentResponse>> Handle(
        GetVendorAssignmentByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _unitOfWork.VendorAssignments.GetByIncidentIdAsync(request.IncidentId, cancellationToken);
        return assignments.Select(a => a.ToResponse()).ToList();
    }
}
