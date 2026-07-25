using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Application.Features.Vendors.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.CreateVendor;

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, VendorResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorResponse> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = new Vendor
        {
            Name = request.VendorName,
            Category = request.VendorCategory,
            ContactPerson = request.ContactPerson,
            ContactEmail = request.Email,
            ContactPhone = request.PhoneNumber,
            IsActive = request.IsActive,
        };

        await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.ToResponse();
    }
}
