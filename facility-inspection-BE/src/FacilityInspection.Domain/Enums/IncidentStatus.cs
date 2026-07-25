namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Lifecycle of an incident as it moves through the MVP workflow:
/// Incident → Inspection → Vendor Assignment → Work Order → Completion → Verification.
/// </summary>
public enum IncidentStatus
{
    PendingInspection = 1,
    UnderInspection = 2,

    /// <summary>Inspection completed with a fault — awaiting vendor assignment.</summary>
    AwaitingVendorAssignment = 3,

    WorkOrderCreated = 4,
    AwaitingVerification = 5,
    Closed = 6,
    Cancelled = 7,

    /// <summary>A vendor has been assigned; awaiting work-order creation.</summary>
    VendorAssigned = 8,
}
