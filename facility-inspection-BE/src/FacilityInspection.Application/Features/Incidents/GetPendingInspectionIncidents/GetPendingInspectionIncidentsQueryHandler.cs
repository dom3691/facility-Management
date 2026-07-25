using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Application.Features.Incidents.Common;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetPendingInspectionIncidents;

public class GetPendingInspectionIncidentsQueryHandler
    : IRequestHandler<GetPendingInspectionIncidentsQuery, PaginatedResult<IncidentListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingInspectionIncidentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<IncidentListResponse>> Handle(
        GetPendingInspectionIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.Incidents.GetPagedAsync(
            pageNumber, pageSize, reportedByUserId: null,
            status: IncidentStatus.PendingInspection, cancellationToken);

        var mapped = items.Select(i => i.ToListResponse()).ToList();
        return PaginatedResult<IncidentListResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
