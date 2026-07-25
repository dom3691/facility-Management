using FacilityInspection.Application.DTOs.Reference;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.CreateLocation;

/// <summary>Creates a location (reference data). Bound from the JSON request body.</summary>
public record CreateLocationCommand : IRequest<LocationResponse>
{
    public Guid FacilityId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Code { get; init; }

    public bool IsActive { get; init; } = true;
}
