namespace FacilityInspection.Application.DTOs.WorkOrders;

/// <summary>Work-order detail returned by the work-order endpoints.</summary>
public record WorkOrderResponse
{
    public Guid Id { get; init; }

    public string WorkOrderNumber { get; init; } = string.Empty;

    public Guid IncidentId { get; init; }

    public string? IncidentNumber { get; init; }

    public Guid VendorAssignmentId { get; init; }

    public Guid VendorId { get; init; }

    public string? VendorName { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTimeOffset CreatedDate { get; init; }

    public DateTimeOffset? CompletedDate { get; init; }
}
