using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.Common;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetIncidents;

public class GetIncidentsQueryHandler
    : IRequestHandler<GetIncidentsQuery, PaginatedResult<IncidentListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetIncidentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<IncidentListResponse>> Handle(
        GetIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Incidents.GetPagedAsync(
            pageNumber, pageSize, reportedByUserId: null, status: request.Status, cancellationToken);

        var mapped = items.Select(i => i.ToListResponse()).ToList();
        return PaginatedResult<IncidentListResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
