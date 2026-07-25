/**
 * Domain enums as string unions — values must match the backend exactly
 * (the API persists/returns enums as strings via `HasConversion<string>()`).
 * Kept in the foundation so feature models can reference them from day one.
 */

export const IncidentStatus = {
  PendingInspection: 'PendingInspection',
  UnderInspection: 'UnderInspection',
  AwaitingVendorAssignment: 'AwaitingVendorAssignment',
  VendorAssigned: 'VendorAssigned',
  WorkOrderCreated: 'WorkOrderCreated',
  AwaitingVerification: 'AwaitingVerification',
  Closed: 'Closed',
  Cancelled: 'Cancelled',
} as const;
export type IncidentStatus = (typeof IncidentStatus)[keyof typeof IncidentStatus];

export const WorkOrderStatus = {
  Open: 'Open',
  Assigned: 'Assigned',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Closed: 'Closed',
  Rejected: 'Rejected',
} as const;
export type WorkOrderStatus = (typeof WorkOrderStatus)[keyof typeof WorkOrderStatus];

export const VerificationDecision = {
  Fixed: 'Fixed',
  NotFixed: 'NotFixed',
} as const;
export type VerificationDecision =
  (typeof VerificationDecision)[keyof typeof VerificationDecision];

export const VendorCategory = {
  InHouse: 'InHouse',
  Leadway: 'Leadway',
  ExternalVendor: 'ExternalVendor',
} as const;
export type VendorCategory = (typeof VendorCategory)[keyof typeof VendorCategory];

export const InspectionClassification = {
  Good: 'Good',
  Faulty: 'Faulty',
  RunDown: 'RunDown',
  Damaged: 'Damaged',
} as const;
export type InspectionClassification =
  (typeof InspectionClassification)[keyof typeof InspectionClassification];
