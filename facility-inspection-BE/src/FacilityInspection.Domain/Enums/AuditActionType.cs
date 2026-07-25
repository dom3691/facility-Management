namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Type of action captured in the audit trail.
/// </summary>
public enum AuditActionType
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    StatusChanged = 4,
    Assigned = 5,
    Verified = 6,
    Login = 7,
    Logout = 8,
}
