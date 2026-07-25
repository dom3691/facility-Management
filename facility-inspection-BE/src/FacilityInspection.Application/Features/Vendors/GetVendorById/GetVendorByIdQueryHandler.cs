using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Application.Features.Vendors.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.GetVendorById;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorResponse> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.Id);

        return vendor.ToResponse();
    }
}
