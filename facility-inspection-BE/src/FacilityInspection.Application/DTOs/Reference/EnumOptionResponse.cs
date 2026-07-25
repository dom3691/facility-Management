namespace FacilityInspection.Application.DTOs.Reference;

/// <summary>A single enum value exposed as a dropdown option (numeric id + name).</summary>
public record EnumOptionResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
