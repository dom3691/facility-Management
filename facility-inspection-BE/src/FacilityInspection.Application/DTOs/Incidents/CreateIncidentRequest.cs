namespace FacilityInspection.Application.DTOs.Incidents;

/// <summary>
/// Form payload for <c>POST /api/incidents</c> (the file attachments are sent as separate
/// multipart parts, not part of this object).
/// </summary>
public class CreateIncidentRequest
{
    public string BusinessUnit { get; set; } = string.Empty;

    public string SAPId { get; set; } = string.Empty;

    public Guid FacilityId { get; set; }

    public Guid LocationId { get; set; }

    public DateTimeOffset IncidentDate { get; set; }

    public string Description { get; set; } = string.Empty;
}
