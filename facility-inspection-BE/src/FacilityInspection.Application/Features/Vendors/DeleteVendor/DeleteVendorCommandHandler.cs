using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.DeleteVendor;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.Id);

        // Soft delete — the save-changes interceptor converts this to a flag update.
        _unitOfWork.Vendors.Delete(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
