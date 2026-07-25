using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.VendorAssignments.Common;

/// <summary>Manual entity → DTO mapping for vendor assignments.</summary>
public static class VendorAssignmentMappings
{
    public static VendorAssignmentResponse ToResponse(this VendorAssignment assignment) => new()
    {
        Id = assignment.Id,
        IncidentId = assignment.IncidentId,
        IncidentNumber = assignment.Incident?.IncidentNumber,
        InspectionId = assignment.InspectionId,
        VendorId = assignment.VendorId,
        VendorName = assignment.Vendor?.Name,
        VendorCategory = assignment.VendorCategory.ToString(),
        AssignedByUserId = assignment.AssignedByUserId,
        AssignedDate = assignment.AssignedDate,
        Notes = assignment.Notes,
        IncidentStatus = assignment.Incident?.Status.ToString(),
        CreatedDate = assignment.CreatedDate,
    };
}
