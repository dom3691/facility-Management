using FacilityInspection.Application.DTOs.Inspections;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetInspectionsByIncident;

/// <summary>Returns all inspections recorded against an incident.</summary>
public record GetInspectionsByIncidentQuery(Guid IncidentId) : IRequest<IReadOnlyList<InspectionResponse>>;
