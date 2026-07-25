using FacilityInspection.Application.DTOs.Reference;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Reference.Common;

/// <summary>Manual entity → DTO mappings for reference data.</summary>
public static class ReferenceMappings
{
    public static FacilityResponse ToResponse(this Facility facility) => new()
    {
        Id = facility.Id,
        Name = facility.Name,
        Code = facility.Code,
        IsActive = facility.IsActive,
    };

    public static LocationResponse ToResponse(this Location location) => new()
    {
        Id = location.Id,
        FacilityId = location.FacilityId,
        Name = location.Name,
        Code = location.Code,
        IsActive = location.IsActive,
    };
}
