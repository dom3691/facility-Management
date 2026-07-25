using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Application.Features.Reference.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.GetFacilities;

public class GetActiveFacilitiesQueryHandler
    : IRequestHandler<GetActiveFacilitiesQuery, IReadOnlyList<FacilityResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveFacilitiesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<FacilityResponse>> Handle(
        GetActiveFacilitiesQuery request,
        CancellationToken cancellationToken)
    {
        var facilities = await _unitOfWork.Facilities.ListAsync(f => f.IsActive, cancellationToken);
        return facilities
            .OrderBy(f => f.Name)
            .Select(f => f.ToResponse())
            .ToList();
    }
}
