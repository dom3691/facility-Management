using FacilityInspection.Application.DTOs.Verifications;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Verifications.Common;

/// <summary>Manual entity → DTO mapping for verifications.</summary>
public static class VerificationMappings
{
    public static VerificationResponse ToResponse(this Verification verification) => new()
    {
        Id = verification.Id,
        WorkOrderId = verification.WorkOrderId,
        WorkOrderNumber = verification.WorkOrder?.WorkOrderNumber,
        IncidentId = verification.WorkOrder?.IncidentId ?? Guid.Empty,
        IncidentNumber = verification.WorkOrder?.Incident?.IncidentNumber,
        VerifiedByUserId = verification.VerifiedByUserId,
        VerificationDate = verification.VerificationDate,
        Decision = verification.Decision.ToString(),
        Comments = verification.Remarks,
        WorkOrderStatus = verification.WorkOrder?.Status.ToString(),
        IncidentStatus = verification.WorkOrder?.Incident?.Status.ToString(),
        CreatedDate = verification.CreatedDate,
    };
}
