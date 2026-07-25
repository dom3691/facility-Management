using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Application.Features.Vendors.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.GetVendors;

public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, PaginatedResult<VendorResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVendorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<VendorResponse>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Vendors.GetPagedAsync(
            pageNumber, pageSize, request.Category, request.IsActive, cancellationToken);

        var mapped = items.Select(v => v.ToResponse()).ToList();
        return PaginatedResult<VendorResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
