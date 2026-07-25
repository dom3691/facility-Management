import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { WorkOrderTimeline } from '../../../../shared/components/work-order-timeline/work-order-timeline';
import { WorkOrderService } from '../../work-order.service';
import { WorkOrder } from '../../work-order.models';

@Component({
  selector: 'app-work-order-details',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
    WorkOrderTimeline,
  ],
  templateUrl: './work-order-details.html',
  styleUrl: './work-order-details.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderDetails {
  private readonly service = inject(WorkOrderService);

  readonly id = input.required<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly workOrder = signal<WorkOrder | null>(null);

  constructor() {
    effect(() => {
      const id = this.id();
      if (id) {
        this.load(id);
      }
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getById(id).subscribe({
      next: (workOrder) => {
        this.workOrder.set(workOrder);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(
          err?.status === 404 ? 'Work order not found.' : (err?.message ?? 'Could not load work order.'),
        );
        this.loading.set(false);
      },
    });
  }
}
