using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetMyIncidents;

public class GetMyIncidentsQueryHandler
    : IRequestHandler<GetMyIncidentsQuery, PaginatedResult<IncidentListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyIncidentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<IncidentListResponse>> Handle(
        GetMyIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Incidents.GetPagedAsync(
            pageNumber, pageSize, reportedByUserId: userId, status: null, cancellationToken);

        var mapped = items.Select(i => i.ToListResponse()).ToList();
        return PaginatedResult<IncidentListResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
