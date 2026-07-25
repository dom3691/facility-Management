using FacilityInspection.Application.DTOs.Incidents;
using MediatR;

namespace FacilityInspection.Application.Features.Incidents.GetIncidentById;

/// <summary>Returns a single incident's full detail, or 404 if it does not exist.</summary>
public record GetIncidentByIdQuery(Guid Id) : IRequest<IncidentResponse>;
