using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetMyIncidents;

/// <summary>Returns a page of incidents reported by the current user.</summary>
public record GetMyIncidentsQuery : IRequest<PaginatedResult<IncidentListResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
