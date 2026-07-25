import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { IncidentService } from '../../../incidents/incident.service';
import { IncidentListItem } from '../../../incidents/incident.models';
import { InspectionService } from '../../inspection.service';
import { Inspection } from '../../inspection.models';

@Component({
  selector: 'app-inspection-queue',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './inspection-queue.html',
  styleUrl: './inspection-queue.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InspectionQueue {
  private readonly incidents = inject(IncidentService);
  private readonly inspections = inject(InspectionService);

  readonly queueColumns = ['incidentNumber', 'businessUnit', 'facility', 'location', 'date', 'actions'];
  readonly mineColumns = ['incidentNumber', 'classification', 'date', 'requiresVendor', 'status', 'actions'];

  // --- Queue (incidents pending inspection) ---
  readonly queueLoading = signal(true);
  readonly queueError = signal<string | null>(null);
  readonly queueItems = signal<IncidentListItem[]>([]);
  readonly queueTotal = signal(0);
  readonly queuePage = signal(0);
  readonly queueSize = signal(10);

  // --- My inspections (lazy loaded on tab open) ---
  readonly mineLoaded = signal(false);
  readonly mineLoading = signal(false);
  readonly mineError = signal<string | null>(null);
  readonly mineItems = signal<Inspection[]>([]);
  readonly mineTotal = signal(0);
  readonly minePage = signal(0);
  readonly mineSize = signal(10);

  constructor() {
    this.loadQueue();
  }

  loadQueue(): void {
    this.queueLoading.set(true);
    this.queueError.set(null);
    this.incidents.pendingInspection(this.queuePage() + 1, this.queueSize()).subscribe({
      next: (result) => {
        this.queueItems.set(result.items);
        this.queueTotal.set(result.totalCount);
        this.queueLoading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.queueError.set(err?.message ?? 'Could not load the inspection queue.');
        this.queueLoading.set(false);
      },
    });
  }

  onQueuePage(event: PageEvent): void {
    this.queuePage.set(event.pageIndex);
    this.queueSize.set(event.pageSize);
    this.loadQueue();
  }

  onTabChange(index: number): void {
    if (index === 1 && !this.mineLoaded()) {
      this.mineLoaded.set(true);
      this.loadMine();
    }
  }

  loadMine(): void {
    this.mineLoading.set(true);
    this.mineError.set(null);
    this.inspections.getMine(this.minePage() + 1, this.mineSize()).subscribe({
      next: (result) => {
        this.mineItems.set(result.items);
        this.mineTotal.set(result.totalCount);
        this.mineLoading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.mineError.set(err?.message ?? 'Could not load your inspections.');
        this.mineLoading.set(false);
      },
    });
  }

  onMinePage(event: PageEvent): void {
    this.minePage.set(event.pageIndex);
    this.mineSize.set(event.pageSize);
    this.loadMine();
  }
}
