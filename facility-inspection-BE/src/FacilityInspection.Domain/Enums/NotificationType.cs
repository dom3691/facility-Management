namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Category of a notification, aligned with the workflow events that raise it.
/// </summary>
public enum NotificationType
{
    IncidentReported = 1,
    InspectionCompleted = 2,
    VendorAssigned = 3,
    WorkOrderCreated = 4,
    WorkOrderCompleted = 5,
    VerificationCompleted = 6,
    General = 7,
}
