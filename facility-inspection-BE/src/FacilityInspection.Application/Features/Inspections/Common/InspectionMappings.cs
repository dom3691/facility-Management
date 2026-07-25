using FacilityInspection.Application.DTOs.Common;
using FacilityInspection.Application.DTOs.Inspections;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Inspections.Common;

/// <summary>Manual entity → DTO mapping for inspections.</summary>
public static class InspectionMappings
{
    public static InspectionResponse ToResponse(this Inspection inspection) => new()
    {
        Id = inspection.Id,
        IncidentId = inspection.IncidentId,
        IncidentNumber = inspection.Incident?.IncidentNumber,
        InspectorUserId = inspection.InspectorUserId,
        InspectionDate = inspection.InspectionDate,
        Classification = inspection.Classification.ToString(),
        Comments = inspection.Findings,
        RequiresVendor = inspection.RequiresVendor,
        IncidentStatus = inspection.Incident?.Status.ToString(),
        CreatedBy = inspection.CreatedBy,
        CreatedDate = inspection.CreatedDate,
        Attachments = inspection.Attachments
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
}
