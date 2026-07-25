using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetIncidentById;

public class GetIncidentByIdQueryHandler
    : IRequestHandler<GetIncidentByIdQuery, IncidentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetIncidentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<IncidentResponse> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        var incident = await _unitOfWork.Incidents.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), request.Id);

        // Object-level check: an initiator may only see their own incidents; a vendor only
        // incidents they hold a work order for; Admin/Inspector see all.
        await IncidentAuthorization.EnsureCanViewAsync(
            incident, _currentUser, _identityService, _unitOfWork, cancellationToken);

        return incident.ToResponse();
    }
}
