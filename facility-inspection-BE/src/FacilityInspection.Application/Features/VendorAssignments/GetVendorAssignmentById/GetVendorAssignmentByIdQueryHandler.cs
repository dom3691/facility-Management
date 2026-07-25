using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Application.Features.VendorAssignments.Common;
using FacilityInspection.Domain.Entities;
using MediatR;

namespace FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentById;

public class GetVendorAssignmentByIdQueryHandler
    : IRequestHandler<GetVendorAssignmentByIdQuery, VendorAssignmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorAssignmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorAssignmentResponse> Handle(
        GetVendorAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.VendorAssignments.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VendorAssignment), request.Id);

        return assignment.ToResponse();
    }
}
