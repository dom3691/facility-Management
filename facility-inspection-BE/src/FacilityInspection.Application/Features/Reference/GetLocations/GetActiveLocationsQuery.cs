using FacilityInspection.Application.DTOs.Reference;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.GetLocations;

/// <summary>Returns active locations for dropdowns, optionally scoped to a facility.</summary>
public record GetActiveLocationsQuery(Guid? FacilityId = null) : IRequest<IReadOnlyList<LocationResponse>>;
