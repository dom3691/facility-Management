import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

interface Stage {
  label: string;
  icon: string;
  state: 'done' | 'active' | 'pending';
}

const STAGES: { label: string; icon: string }[] = [
  { label: 'Reported', icon: 'report_problem' },
  { label: 'Inspected', icon: 'fact_check' },
  { label: 'Vendor Assigned', icon: 'assignment_ind' },
  { label: 'Work Order', icon: 'engineering' },
  { label: 'Verification', icon: 'verified' },
  { label: 'Closed', icon: 'check_circle' },
];

/** Maps an incident status onto the timeline's stage index. */
const STATUS_STAGE: Record<string, number> = {
  PendingInspection: 0,
  UnderInspection: 0,
  AwaitingVendorAssignment: 1,
  VendorAssigned: 2,
  WorkOrderCreated: 3,
  AwaitingVerification: 4,
  Closed: 5,
};

/**
 * Horizontal workflow timeline. Given the incident's current status, marks each
 * stage as done / active / pending. Reusable across incident, inspection and
 * work-order detail views. `Cancelled` renders a distinct terminal state.
 */
@Component({
  selector: 'app-status-timeline',
  imports: [MatIconModule],
  templateUrl: './status-timeline.html',
  styleUrl: './status-timeline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusTimeline {
  readonly status = input.required<string>();

  readonly cancelled = computed(() => this.status() === 'Cancelled');

  readonly stages = computed<Stage[]>(() => {
    const current = STATUS_STAGE[this.status()] ?? 0;
    return STAGES.map((stage, index) => ({
      ...stage,
      state: index < current ? 'done' : index === current ? 'active' : 'pending',
    }));
  });
}
