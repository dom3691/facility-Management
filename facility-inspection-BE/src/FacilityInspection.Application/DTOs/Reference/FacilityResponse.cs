namespace FacilityInspection.Application.DTOs.Reference;

/// <summary>A facility option for reference/dropdown APIs.</summary>
public record FacilityResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
