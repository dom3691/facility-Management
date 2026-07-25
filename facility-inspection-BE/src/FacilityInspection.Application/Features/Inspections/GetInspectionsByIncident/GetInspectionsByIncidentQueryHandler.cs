using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Application.Features.Inspections.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetInspectionsByIncident;

public class GetInspectionsByIncidentQueryHandler
    : IRequestHandler<GetInspectionsByIncidentQuery, IReadOnlyList<InspectionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetInspectionsByIncidentQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InspectionResponse>> Handle(
        GetInspectionsByIncidentQuery request,
        CancellationToken cancellationToken)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(request.IncidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), request.IncidentId);

        InspectionAuthorization.EnsureCanViewIncidentInspections(incident, _currentUser);

        var inspections = await _unitOfWork.Inspections.GetByIncidentIdAsync(request.IncidentId, cancellationToken);
        return inspections.Select(i => i.ToResponse()).ToList();
    }
}
