using FacilityInspection.Application.DTOs.VendorAssignments;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.VendorAssignments.CreateVendorAssignment;

/// <summary>Assigns a vendor to an inspected incident. Bound from the JSON request body.</summary>
public record CreateVendorAssignmentCommand : IRequest<VendorAssignmentResponse>
{
    public Guid IncidentId { get; init; }

    public Guid InspectionId { get; init; }

    public Guid VendorId { get; init; }

    public VendorCategory VendorCategory { get; init; }

    public string? Notes { get; init; }
}
