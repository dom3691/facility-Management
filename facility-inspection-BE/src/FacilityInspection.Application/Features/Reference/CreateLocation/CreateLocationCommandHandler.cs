using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Application.Features.Reference.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.CreateLocation;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, LocationResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLocationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LocationResponse> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // The location's facility must exist.
        _ = await _unitOfWork.Facilities.GetByIdAsync(request.FacilityId, cancellationToken)
            ?? throw new NotFoundException(nameof(Facility), request.FacilityId);

        var location = new Location
        {
            FacilityId = request.FacilityId,
            Name = request.Name,
            Code = request.Code,
            IsActive = request.IsActive,
        };

        await _unitOfWork.Repository<Location>().AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return location.ToResponse();
    }
}
