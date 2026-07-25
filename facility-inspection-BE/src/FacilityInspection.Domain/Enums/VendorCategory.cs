namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Classification of a vendor that can be assigned to remediate an incident.
/// </summary>
public enum VendorCategory
{
    InHouse = 1,
    Leadway = 2,
    ExternalVendor = 3
}
