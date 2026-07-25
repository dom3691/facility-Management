/** Mirrors the backend `WorkOrderResponse`. */
export interface WorkOrder {
  id: string;
  workOrderNumber: string;
  incidentId: string;
  incidentNumber?: string | null;
  vendorAssignmentId: string;
  vendorId: string;
  vendorName?: string | null;
  status: string;
  description?: string | null;
  createdDate: string;
  completedDate?: string | null;
}

/** Status filter options (values match the backend `WorkOrderStatus` enum). */
export const WORK_ORDER_STATUS_OPTIONS: { value: string; label: string }[] = [
  { value: '', label: 'All statuses' },
  { value: 'Open', label: 'Open' },
  { value: 'Assigned', label: 'Assigned' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Closed', label: 'Closed' },
  { value: 'Rejected', label: 'Rejected' },
];
