using FacilityInspection.Application.DTOs.VendorAssignments;
using MediatR;

namespace FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentByIncident;

/// <summary>Returns all vendor assignments for an incident.</summary>
public record GetVendorAssignmentByIncidentQuery(Guid IncidentId)
    : IRequest<IReadOnlyList<VendorAssignmentResponse>>;
