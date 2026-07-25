using FacilityInspection.Application.DTOs.Inspections;
using MediatR;

namespace FacilityInspection.Application.Features.Inspections.GetInspectionById;

/// <summary>Returns a single inspection's detail, or 404 if it does not exist.</summary>
public record GetInspectionByIdQuery(Guid Id) : IRequest<InspectionResponse>;
