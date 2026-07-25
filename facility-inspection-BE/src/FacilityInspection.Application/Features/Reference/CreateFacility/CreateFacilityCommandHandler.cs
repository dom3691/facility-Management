using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Application.Features.Reference.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.CreateFacility;

public class CreateFacilityCommandHandler : IRequestHandler<CreateFacilityCommand, FacilityResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFacilityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FacilityResponse> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Facilities.ListAsync(f => f.Code == request.Code, cancellationToken);
        if (existing.Count > 0)
        {
            throw new BadRequestException($"A facility with code '{request.Code}' already exists.");
        }

        var facility = new Facility
        {
            Name = request.Name,
            Code = request.Code,
            IsActive = request.IsActive,
        };

        await _unitOfWork.Facilities.AddAsync(facility, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return facility.ToResponse();
    }
}
