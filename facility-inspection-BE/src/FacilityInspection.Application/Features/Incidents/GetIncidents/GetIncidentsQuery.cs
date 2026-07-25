using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetIncidents;

/// <summary>Returns a page of incidents, optionally filtered by status.</summary>
public record GetIncidentsQuery : IRequest<PaginatedResult<IncidentListResponse>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public IncidentStatus? Status { get; init; }
}
