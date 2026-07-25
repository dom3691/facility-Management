import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

interface Step {
  label: string;
  icon: string;
  date: string | null;
  state: 'done' | 'active' | 'pending';
}

/** Maps a work-order status to the active stage index. */
const STATUS_STEP: Record<string, number> = {
  Open: 0,
  Assigned: 0,
  InProgress: 1,
  Completed: 2,
  Closed: 3,
};

/**
 * Vertical work-order lifecycle timeline: Assigned → In Progress → Completed →
 * Closed, with dates where known. `Rejected` renders a distinct terminal state.
 */
@Component({
  selector: 'app-work-order-timeline',
  imports: [DatePipe, MatIconModule],
  templateUrl: './work-order-timeline.html',
  styleUrl: './work-order-timeline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderTimeline {
  readonly status = input.required<string>();
  readonly createdDate = input<string | null>(null);
  readonly completedDate = input<string | null>(null);

  readonly rejected = computed(() => this.status() === 'Rejected');

  readonly steps = computed<Step[]>(() => {
    const current = STATUS_STEP[this.status()] ?? 0;
    const defs = [
      { label: 'Assigned', icon: 'assignment_turned_in', date: this.createdDate() },
      { label: 'In Progress', icon: 'engineering', date: null },
      { label: 'Completed', icon: 'task_alt', date: this.completedDate() },
      { label: 'Closed', icon: 'check_circle', date: null },
    ];
    return defs.map((def, index) => ({
      ...def,
      state: index < current ? 'done' : index === current ? 'active' : 'pending',
    }));
  });
}
