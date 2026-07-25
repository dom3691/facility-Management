import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

interface ChipMeta {
  label: string;
  tone: string;
}

/** Status → display label + colour tone. Covers incident and work-order statuses. */
const STATUS_META: Record<string, ChipMeta> = {
  // Incident
  PendingInspection: { label: 'Pending Inspection', tone: 'amber' },
  UnderInspection: { label: 'Under Inspection', tone: 'blue' },
  AwaitingVendorAssignment: { label: 'Awaiting Vendor', tone: 'orange' },
  VendorAssigned: { label: 'Vendor Assigned', tone: 'violet' },
  WorkOrderCreated: { label: 'Work Order Created', tone: 'indigo' },
  AwaitingVerification: { label: 'Awaiting Verification', tone: 'teal' },
  Closed: { label: 'Closed', tone: 'green' },
  Cancelled: { label: 'Cancelled', tone: 'grey' },
  // Work order (shared component, reused later)
  Open: { label: 'Open', tone: 'blue' },
  Assigned: { label: 'Assigned', tone: 'violet' },
  InProgress: { label: 'In Progress', tone: 'orange' },
  Completed: { label: 'Completed', tone: 'teal' },
  Rejected: { label: 'Rejected', tone: 'red' },
  // Inspection classification
  Good: { label: 'Good', tone: 'green' },
  Faulty: { label: 'Faulty', tone: 'amber' },
  RunDown: { label: 'Run Down', tone: 'orange' },
  Damaged: { label: 'Damaged', tone: 'red' },
  // Verification decision
  Fixed: { label: 'Fixed', tone: 'green' },
  NotFixed: { label: 'Not Fixed', tone: 'red' },
};

/**
 * Small pill showing a workflow status with a colour tone. Reusable across
 * incidents, work orders and verifications.
 */
@Component({
  selector: 'app-status-chip',
  template: `<span class="chip" [attr.data-tone]="meta().tone">{{ meta().label }}</span>`,
  styleUrl: './status-chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusChip {
  readonly status = input.required<string>();

  readonly meta = computed<ChipMeta>(
    () => STATUS_META[this.status()] ?? { label: this.status(), tone: 'grey' },
  );
}
