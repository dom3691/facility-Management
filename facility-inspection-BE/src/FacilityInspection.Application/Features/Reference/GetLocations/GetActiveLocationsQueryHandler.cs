using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Application.Features.Reference.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.GetLocations;

public class GetActiveLocationsQueryHandler
    : IRequestHandler<GetActiveLocationsQuery, IReadOnlyList<LocationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveLocationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LocationResponse>> Handle(
        GetActiveLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var facilityId = request.FacilityId;

        var locations = await _unitOfWork.Repository<Location>().ListAsync(
            l => l.IsActive && (facilityId == null || l.FacilityId == facilityId),
            cancellationToken);

        return locations
            .OrderBy(l => l.Name)
            .Select(l => l.ToResponse())
            .ToList();
    }
}
