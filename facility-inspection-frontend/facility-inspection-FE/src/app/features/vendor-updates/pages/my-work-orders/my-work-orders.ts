import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { WorkOrderService } from '../../../work-orders/work-order.service';
import { WorkOrder } from '../../../work-orders/work-order.models';

type Bucket = 'all' | 'assigned' | 'active' | 'completed';
type ViewMode = 'cards' | 'table';

const ASSIGNED = ['Assigned', 'Open'];
const ACTIVE = ['InProgress'];
const COMPLETED = ['Completed', 'Closed'];

/** Vendor portal home: the vendor's work orders bucketed by job state. */
@Component({
  selector: 'app-my-work-orders',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatButtonToggleModule,
    MatTableModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './my-work-orders.html',
  styleUrl: './my-work-orders.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyWorkOrders {
  private readonly service = inject(WorkOrderService);

  readonly columns = ['workOrderNumber', 'incident', 'status', 'created', 'actions'];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<WorkOrder[]>([]);

  readonly bucket = signal<Bucket>('all');
  readonly view = signal<ViewMode>('cards');

  readonly assignedJobs = computed(() => this.items().filter((w) => ASSIGNED.includes(w.status)));
  readonly activeJobs = computed(() => this.items().filter((w) => ACTIVE.includes(w.status)));
  readonly completedJobs = computed(() => this.items().filter((w) => COMPLETED.includes(w.status)));

  readonly visibleJobs = computed(() => {
    switch (this.bucket()) {
      case 'assigned':
        return this.assignedJobs();
      case 'active':
        return this.activeJobs();
      case 'completed':
        return this.completedJobs();
      default:
        return this.items();
    }
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    // Vendors typically have a manageable number of jobs — load them and bucket client-side.
    this.service.myVendor(1, 100).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load your work orders.');
        this.loading.set(false);
      },
    });
  }

  setBucket(bucket: Bucket): void {
    this.bucket.set(bucket);
  }

  isActionable(status: string): boolean {
    return ASSIGNED.includes(status) || ACTIVE.includes(status);
  }
}
