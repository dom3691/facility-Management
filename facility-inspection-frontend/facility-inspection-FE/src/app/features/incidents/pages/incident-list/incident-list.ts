import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { AppRole, IncidentStatus } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { IncidentService } from '../../incident.service';
import { IncidentListItem } from '../../incident.models';

@Component({
  selector: 'app-incident-list',
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './incident-list.html',
  styleUrl: './incident-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IncidentList {
  private readonly service = inject(IncidentService);
  private readonly auth = inject(AuthService);

  readonly displayedColumns = [
    'incidentNumber',
    'businessUnit',
    'facility',
    'location',
    'date',
    'status',
    'actions',
  ];

  readonly statusOptions: { value: string; label: string }[] = [
    { value: '', label: 'All statuses' },
    { value: IncidentStatus.PendingInspection, label: 'Pending Inspection' },
    { value: IncidentStatus.UnderInspection, label: 'Under Inspection' },
    { value: IncidentStatus.AwaitingVendorAssignment, label: 'Awaiting Vendor' },
    { value: IncidentStatus.VendorAssigned, label: 'Vendor Assigned' },
    { value: IncidentStatus.WorkOrderCreated, label: 'Work Order Created' },
    { value: IncidentStatus.AwaitingVerification, label: 'Awaiting Verification' },
    { value: IncidentStatus.Closed, label: 'Closed' },
    { value: IncidentStatus.Cancelled, label: 'Cancelled' },
  ];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<IncidentListItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);

  readonly search = signal('');
  readonly statusFilter = signal('');

  /** Admin/Inspector see all incidents (with server-side status filter); others see their own. */
  private readonly seesAll = computed(() =>
    this.auth.hasAnyRole(AppRole.Admin, AppRole.Inspector),
  );
  readonly canCreate = computed(() => this.auth.hasAnyRole(AppRole.Admin, AppRole.Initiator));
  readonly scopeLabel = computed(() =>
    this.seesAll() ? 'All reported incidents' : 'Incidents you reported',
  );

  /** Client-side view: search always; status only when not filtered server-side. */
  readonly visibleRows = computed(() => {
    const term = this.search().trim().toLowerCase();
    const status = this.statusFilter();
    return this.items().filter((row) => {
      const matchesSearch =
        !term ||
        [row.incidentNumber, row.businessUnit, row.facilityName, row.locationName]
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
      ? this.service.list(page, size, (this.statusFilter() || null) as IncidentStatus | null)
      : this.service.listMine(page, size);

    request$.subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load incidents.');
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
    // Server-side filter needs a refetch (page 1); client-side just recomputes.
    if (this.seesAll()) {
      this.pageIndex.set(0);
      this.load();
    }
  }
}
