namespace FacilityInspection.Application.DTOs.Dashboard;

/// <summary>Basic operational counters for the MVP dashboard (role-scoped).</summary>
public record DashboardSummaryResponse
{
    public int TotalIncidents { get; init; }

    public int PendingInspectionCount { get; init; }

    public int InspectionCompletedCount { get; init; }

    public int VendorAssignedCount { get; init; }

    public int WorkOrdersOpenCount { get; init; }

    public int WorkOrdersInProgressCount { get; init; }

    public int WorkOrdersCompletedCount { get; init; }

    public int WorkOrdersClosedCount { get; init; }

    public int PendingVerificationCount { get; init; }
}
