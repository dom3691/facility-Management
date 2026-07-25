/**
 * Mirrors the backend `DashboardSummaryResponse` (`GET /api/dashboard/summary`).
 * The API returns these counters already scoped to the caller's role
 * (Admin/Inspector = all, Initiator = own incidents, Vendor = own work orders).
 */
export interface DashboardSummary {
  totalIncidents: number;
  pendingInspectionCount: number;
  inspectionCompletedCount: number;
  vendorAssignedCount: number;
  workOrdersOpenCount: number;
  workOrdersInProgressCount: number;
  workOrdersCompletedCount: number;
  workOrdersClosedCount: number;
  pendingVerificationCount: number;
}
