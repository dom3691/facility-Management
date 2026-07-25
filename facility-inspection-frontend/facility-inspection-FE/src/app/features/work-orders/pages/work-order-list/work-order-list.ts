import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { AppRole } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { WorkOrderService } from '../../work-order.service';
import { WorkOrder, WORK_ORDER_STATUS_OPTIONS } from '../../work-order.models';

@Component({
  selector: 'app-work-order-list',
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './work-order-list.html',
  styleUrl: './work-order-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderList {
  private readonly service = inject(WorkOrderService);
  private readonly auth = inject(AuthService);

  readonly displayedColumns = ['workOrderNumber', 'incident', 'vendor', 'status', 'created', 'actions'];
  readonly statusOptions = WORK_ORDER_STATUS_OPTIONS;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<WorkOrder[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly search = signal('');
  readonly statusFilter = signal('');

  /** Admin/Inspector see all (with server status filter); a Vendor sees their own. */
  private readonly seesAll = computed(() =>
    this.auth.hasAnyRole(AppRole.Admin, AppRole.Inspector),
  );
  readonly scopeLabel = computed(() =>
    this.seesAll() ? 'All work orders' : 'Work orders assigned to your vendor',
  );

  readonly visibleRows = computed(() => {
    const term = this.search().trim().toLowerCase();
    const status = this.statusFilter();
    return this.items().filter((row) => {
      const matchesSearch =
        !term ||
        [row.workOrderNumber, row.vendorName, row.incidentNumber]
          .filter(Boolean)
          .some((v) => v!.toLowerCase().includes(term));
      const matchesStatus = this.seesAll() || !status || row.status === status;
      return matchesSearch && matchesStatus;
    });
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    const page = this.pageIndex() + 1;
    const size = this.pageSize();
    const request$ = this.seesAll()
      ? this.service.list(page, size, this.statusFilter() || null)
      : this.service.myVendor(page, size);

    request$.subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load work orders.');
        this.loading.set(false);
      },
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onStatusChange(): void {
    if (this.seesAll()) {
      this.pageIndex.set(0);
      this.load();
    }
  }
}
