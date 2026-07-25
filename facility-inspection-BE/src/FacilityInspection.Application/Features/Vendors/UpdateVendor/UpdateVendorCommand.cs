using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.UpdateVendor;

/// <summary>Updates a vendor. <see cref="Id"/> is taken from the route.</summary>
public record UpdateVendorCommand : IRequest<VendorResponse>
{
    public Guid Id { get; init; }

    public string VendorName { get; init; } = string.Empty;

    public VendorCategory VendorCategory { get; init; }

    public string? ContactPerson { get; init; }

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }
}
