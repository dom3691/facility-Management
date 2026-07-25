using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetPendingInspectionIncidents;

/// <summary>Returns a page of incidents awaiting inspection (the inspection queue).</summary>
public record GetPendingInspectionIncidentsQuery : IRequest<PaginatedResult<IncidentListResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
