namespace FacilityInspection.Application.DTOs.VendorAssignments;

/// <summary>Vendor-assignment detail returned by the assignment endpoints.</summary>
public record VendorAssignmentResponse
{
    public Guid Id { get; init; }

    public Guid IncidentId { get; init; }

    public string? IncidentNumber { get; init; }

    public Guid InspectionId { get; init; }

    public Guid VendorId { get; init; }

    public string? VendorName { get; init; }

    public string VendorCategory { get; init; } = string.Empty;

    public Guid AssignedByUserId { get; init; }

    public DateTimeOffset AssignedDate { get; init; }

    public string? Notes { get; init; }

    /// <summary>The incident's status after the assignment.</summary>
    public string? IncidentStatus { get; init; }

    public DateTimeOffset CreatedDate { get; init; }
}
