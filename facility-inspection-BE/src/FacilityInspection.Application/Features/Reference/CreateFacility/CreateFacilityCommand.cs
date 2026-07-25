using FacilityInspection.Application.DTOs.Reference;
using MediatR;

namespace FacilityInspection.Application.Features.Reference.CreateFacility;

/// <summary>Creates a facility (reference data). Bound from the JSON request body.</summary>
public record CreateFacilityCommand : IRequest<FacilityResponse>
{
    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
