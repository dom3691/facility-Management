using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Application.Features.Inspections.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetMyInspections;

public class GetMyInspectionsQueryHandler
    : IRequestHandler<GetMyInspectionsQuery, PaginatedResult<InspectionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyInspectionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<InspectionResponse>> Handle(
        GetMyInspectionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Inspections.GetPagedByInspectorAsync(
            userId, pageNumber, pageSize, cancellationToken);

        var mapped = items.Select(i => i.ToResponse()).ToList();
        return PaginatedResult<InspectionResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
