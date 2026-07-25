using FacilityInspection.Application.DTOs.Common;

namespace FacilityInspection.Application.DTOs.Inspections;

/// <summary>Full inspection detail returned by create, get-by-id, by-incident and my.</summary>
public record InspectionResponse
{
    public Guid Id { get; init; }

    public Guid IncidentId { get; init; }

    public string? IncidentNumber { get; init; }

    public Guid InspectorUserId { get; init; }

    public DateTimeOffset InspectionDate { get; init; }

    public string Classification { get; init; } = string.Empty;

    public string Comments { get; init; } = string.Empty;

    public bool RequiresVendor { get; init; }

    /// <summary>The incident's status after this inspection was recorded.</summary>
    public string? IncidentStatus { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset CreatedDate { get; init; }

    public IReadOnlyList<AttachmentResponse> Attachments { get; init; }
        = Array.Empty<AttachmentResponse>();
}
