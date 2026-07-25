namespace FacilityInspection.Application.DTOs.Vendors;

/// <summary>Vendor detail returned by the vendor endpoints.</summary>
public record VendorResponse
{
    public Guid Id { get; init; }

    public string VendorName { get; init; } = string.Empty;

    public string VendorCategory { get; init; } = string.Empty;

    public string? ContactPerson { get; init; }

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedDate { get; init; }
}
