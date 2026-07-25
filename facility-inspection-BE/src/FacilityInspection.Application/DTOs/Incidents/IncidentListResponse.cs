namespace FacilityInspection.Application.DTOs.Incidents;

/// <summary>Lightweight incident row for list/queue views.</summary>
public record IncidentListResponse
{
    public Guid Id { get; init; }

    public string IncidentNumber { get; init; } = string.Empty;

    public string BusinessUnit { get; init; } = string.Empty;

    public string? FacilityName { get; init; }

    public string? LocationName { get; init; }

    public DateTimeOffset IncidentDate { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedDate { get; init; }
}
