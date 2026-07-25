import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { IncidentStatus } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { IncidentService } from '../../../incidents/incident.service';
import { IncidentListItem } from '../../../incidents/incident.models';

/** Incidents awaiting vendor assignment — the entry point for assigning vendors. */
@Component({
  selector: 'app-assignment-queue',
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
  templateUrl: './assignment-queue.html',
  styleUrl: './assignment-queue.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssignmentQueue {
  private readonly incidents = inject(IncidentService);

  readonly columns = ['incidentNumber', 'businessUnit', 'facility', 'location', 'date', 'actions'];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<IncidentListItem[]>([]);
  readonly total = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.incidents
      .list(this.pageIndex() + 1, this.pageSize(), IncidentStatus.AwaitingVendorAssignment)
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.total.set(result.totalCount);
          this.loading.set(false);
        },
        error: (err: NormalizedHttpError) => {
          this.error.set(err?.message ?? 'Could not load incidents awaiting assignment.');
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
