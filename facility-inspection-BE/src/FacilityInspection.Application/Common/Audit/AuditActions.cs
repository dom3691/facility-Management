namespace FacilityInspection.Application.Common.Audit;

/// <summary>Canonical audit action names (avoids magic strings across handlers).</summary>
public static class AuditActions
{
    // Generic CRUD
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";

    // Workflow actions
    public const string IncidentCreated = "Incident Created";
    public const string InspectionCompleted = "Inspection Completed";
    public const string VendorAssigned = "Vendor Assigned";
    public const string WorkOrderGenerated = "Work Order Generated";
    public const string WorkOrderStatusChanged = "Work Order Status Changed";
    public const string VendorProgressUpdate = "Vendor Progress Update";
    public const string VendorMarkedComplete = "Vendor Marked Complete";
    public const string VerificationFixed = "Verification Fixed";
    public const string VerificationNotFixed = "Verification Not Fixed";
}
