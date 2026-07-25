using FacilityInspection.Application.DTOs.Vendors;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.GetVendorById;

/// <summary>Returns a single vendor, or 404 if it does not exist.</summary>
public record GetVendorByIdQuery(Guid Id) : IRequest<VendorResponse>;
