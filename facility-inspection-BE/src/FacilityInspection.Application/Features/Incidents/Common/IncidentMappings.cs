using FacilityInspection.Application.DTOs.Common;
using FacilityInspection.Application.DTOs.Incidents;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Incidents.Common;

/// <summary>
/// Manual entity → DTO mappings for incidents. (Swap for AutoMapper profiles later if desired.)
/// </summary>
public static class IncidentMappings
{
    public static IncidentResponse ToResponse(this Incident incident) => new()
    {
        Id = incident.Id,
        IncidentNumber = incident.IncidentNumber,
        BusinessUnit = incident.BusinessUnit,
        SAPId = incident.SAPId,
        FacilityId = incident.FacilityId,
        FacilityName = incident.Facility?.Name,
        LocationId = incident.LocationId,
        LocationName = incident.Location?.Name,
        IncidentDate = incident.IncidentDate,
        Description = incident.Description,
        Status = incident.Status.ToString(),
        ReportedByUserId = incident.ReportedByUserId,
        CreatedBy = incident.CreatedBy,
        CreatedDate = incident.CreatedDate,
        Attachments = incident.Attachments
            .Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                StoragePath = a.StoragePath,
            })
            .ToList(),
    };

    public static IncidentListResponse ToListResponse(this Incident incident) => new()
    {
        Id = incident.Id,
        IncidentNumber = incident.IncidentNumber,
        BusinessUnit = incident.BusinessUnit,
        FacilityName = incident.Facility?.Name,
        LocationName = incident.Location?.Name,
        IncidentDate = incident.IncidentDate,
        Status = incident.Status.ToString(),
        CreatedDate = incident.CreatedDate,
    };
}
