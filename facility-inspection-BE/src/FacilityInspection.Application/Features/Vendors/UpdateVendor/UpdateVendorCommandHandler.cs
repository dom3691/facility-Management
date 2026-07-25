using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Application.Features.Vendors.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.UpdateVendor;

public class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand, VendorResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorResponse> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.Id);

        vendor.Name = request.VendorName;
        vendor.Category = request.VendorCategory;
        vendor.ContactPerson = request.ContactPerson;
        vendor.ContactEmail = request.Email;
        vendor.ContactPhone = request.PhoneNumber;
        vendor.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.ToResponse();
    }
}
