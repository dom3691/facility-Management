using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Domain.Enums;
using MediatR;

namespace FacilityInspection.Application.Features.Vendors.CreateVendor;

/// <summary>Creates a vendor (master data). Bound directly from the JSON request body.</summary>
public record CreateVendorCommand : IRequest<VendorResponse>
{
    public string VendorName { get; init; } = string.Empty;

    public VendorCategory VendorCategory { get; init; }

    public string? ContactPerson { get; init; }

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; } = true;
}
