namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Lifecycle of a work order issued to a vendor to remediate an incident.
/// </summary>
public enum WorkOrderStatus
{
    Open = 1,
    Assigned = 2,
    InProgress = 3,
    Completed = 4,
    Closed = 5,
    Rejected = 6,
}
