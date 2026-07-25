using FacilityInspection.Application.DTOs.Vendors;
using FacilityInspection.Domain.Entities;

namespace FacilityInspection.Application.Features.Vendors.Common;

/// <summary>Manual entity → DTO mapping for vendors.</summary>
public static class VendorMappings
{
    public static VendorResponse ToResponse(this Vendor vendor) => new()
    {
        Id = vendor.Id,
        VendorName = vendor.Name,
        VendorCategory = vendor.Category.ToString(),
        ContactPerson = vendor.ContactPerson,
        Email = vendor.ContactEmail,
        PhoneNumber = vendor.ContactPhone,
        IsActive = vendor.IsActive,
        CreatedDate = vendor.CreatedDate,
    };
}
