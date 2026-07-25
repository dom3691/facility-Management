using FacilityInspection.Application.DTOs.WorkOrders;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.WorkOrders.Common;

/// <summary>Manual entity → DTO mapping for work orders.</summary>
public static class WorkOrderMappings
{
    public static WorkOrderResponse ToResponse(this WorkOrder workOrder) => new()
    {
        Id = workOrder.Id,
        WorkOrderNumber = workOrder.WorkOrderNumber,
        IncidentId = workOrder.IncidentId,
        IncidentNumber = workOrder.Incident?.IncidentNumber,
        VendorAssignmentId = workOrder.VendorAssignmentId,
        VendorId = workOrder.VendorId,
        VendorName = workOrder.Vendor?.Name,
        Status = workOrder.Status.ToString(),
        Description = workOrder.Description,
        CreatedDate = workOrder.CreatedDate,
        CompletedDate = workOrder.CompletedDate,
    };
}
