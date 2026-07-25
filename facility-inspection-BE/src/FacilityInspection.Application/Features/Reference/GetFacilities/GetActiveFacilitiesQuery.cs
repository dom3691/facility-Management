using FacilityInspection.Application.DTOs.Reference;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.GetFacilities;

/// <summary>Returns active facilities for dropdowns.</summary>
public record GetActiveFacilitiesQuery : IRequest<IReadOnlyList<FacilityResponse>>;
