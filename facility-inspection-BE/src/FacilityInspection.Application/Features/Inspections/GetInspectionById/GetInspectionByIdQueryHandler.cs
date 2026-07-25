using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Application.Features.Inspections.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetInspectionById;

public class GetInspectionByIdQueryHandler
    : IRequestHandler<GetInspectionByIdQuery, InspectionResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetInspectionByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<InspectionResponse> Handle(GetInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var inspection = await _unitOfWork.Inspections.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Inspection), request.Id);

        InspectionAuthorization.EnsureCanView(inspection, _currentUser);

        return inspection.ToResponse();
    }
}
