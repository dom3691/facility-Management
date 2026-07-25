import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { WorkOrder } from '../../../work-orders/work-order.models';
import { VerificationService } from '../../verification.service';

/** Completed work orders awaiting the initiator's verification. */
@Component({
  selector: 'app-pending-verification',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    PageHeader,
  ],
  templateUrl: './pending-verification.html',
  styleUrl: './pending-verification.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PendingVerification {
  private readonly service = inject(VerificationService);

  readonly columns = ['workOrderNumber', 'incident', 'vendor', 'completed', 'actions'];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<WorkOrder[]>([]);
  readonly total = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getPending(this.pageIndex() + 1, this.pageSize()).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load pending verifications.');
        this.loading.set(false);
      },
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }
}
