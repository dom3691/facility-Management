using FacilityInspection.Application.DTOs.Common;

namespace FacilityInspection.Application.DTOs.Incidents;

/// <summary>Full incident detail returned by create and get-by-id.</summary>
public record IncidentResponse
{
    public Guid Id { get; init; }

    public string IncidentNumber { get; init; } = string.Empty;

    public string BusinessUnit { get; init; } = string.Empty;

    public string SAPId { get; init; } = string.Empty;

    public Guid FacilityId { get; init; }

    public string? FacilityName { get; init; }

    public Guid LocationId { get; init; }

    public string? LocationName { get; init; }

    public DateTimeOffset IncidentDate { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public Guid ReportedByUserId { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset CreatedDate { get; init; }

    public IReadOnlyList<AttachmentResponse> Attachments { get; init; }
        = Array.Empty<AttachmentResponse>();
}
