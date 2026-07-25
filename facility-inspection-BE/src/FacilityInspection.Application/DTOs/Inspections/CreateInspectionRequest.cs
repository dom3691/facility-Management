using FacilityInspection.Domain.Enums;

namespace FacilityInspection.Application.DTOs.Inspections;

/// <summary>
/// Form payload for <c>POST /api/inspections</c> (photo attachments are sent as separate
/// multipart parts).
/// </summary>
public class CreateInspectionRequest
{
    public Guid IncidentId { get; set; }

    public InspectionClassification Classification { get; set; }

    public string Comments { get; set; } = string.Empty;
}
