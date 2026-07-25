namespace FacilityInspection.Application.DTOs.Reference;

/// <summary>A location option for reference/dropdown APIs.</summary>
public record LocationResponse
{
    public Guid Id { get; init; }

    public Guid FacilityId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Code { get; init; }

    public bool IsActive { get; init; }
}
