using FacilityInspection.Application.DTOs.VendorAssignments;
using MediatR;

namespace FacilityInspection.Application.Features.VendorAssignments.GetVendorAssignmentById;

/// <summary>Returns a single vendor assignment, or 404 if it does not exist.</summary>
public record GetVendorAssignmentByIdQuery(Guid Id) : IRequest<VendorAssignmentResponse>;
