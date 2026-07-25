import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChartConfiguration } from 'chart.js';

import { AuthService } from '../../../../core/services/auth.service';
import { AppRole } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { ChartComponent } from '../../../../shared/components/chart/chart';
import { DashboardService } from '../../dashboard.service';
import { DashboardSummary } from '../../dashboard.models';

interface StatCard {
  label: string;
  value: number;
  icon: string;
  accent: string;
}

/** Colours for the categorical charts (accessible, brand-adjacent). */
const CHART_COLORS = {
  open: '#2563eb',
  inProgress: '#d97706',
  completed: '#16a34a',
  closed: '#0d1b4b',
  pendingInspection: '#f59e0b',
  inspectionDone: '#0ea5e9',
  vendorAssigned: '#7c3aed',
  pendingVerification: '#c74b30',
};

@Component({
  selector: 'app-dashboard',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    ChartComponent,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  private readonly service = inject(DashboardService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly summary = signal<DashboardSummary | null>(null);

  readonly user = this.auth.currentUser;

  /** Role-aware subtitle: the API already scopes the numbers; this names the scope. */
  readonly scopeLabel = computed(() => {
    const roles = this.auth.roles();
    if (roles.includes(AppRole.Admin) || roles.includes(AppRole.Inspector)) {
      return 'Organisation-wide overview';
    }
    if (roles.includes(AppRole.Vendor)) {
      return 'Your assigned work orders';
    }
    return 'Your reported incidents';
  });

  readonly cards = computed<StatCard[]>(() => {
    const s = this.summary();
    if (!s) {
      return [];
    }
    return [
      { label: 'Total Incidents', value: s.totalIncidents, icon: 'summarize', accent: 'blue' },
      { label: 'Pending Inspection', value: s.pendingInspectionCount, icon: 'pending_actions', accent: 'amber' },
      { label: 'Vendor Assigned', value: s.vendorAssignedCount, icon: 'assignment_ind', accent: 'violet' },
      { label: 'Open Work Orders', value: s.workOrdersOpenCount, icon: 'build_circle', accent: 'cyan' },
      { label: 'Completed Work Orders', value: s.workOrdersCompletedCount, icon: 'task_alt', accent: 'green' },
      { label: 'Pending Verification', value: s.pendingVerificationCount, icon: 'verified', accent: 'rust' },
    ];
  });

  /** Doughnut — work order status distribution. */
  readonly workOrderChart = computed<ChartConfiguration | null>(() => {
    const s = this.summary();
    if (!s) {
      return null;
    }
    return {
      type: 'doughnut',
      data: {
        labels: ['Open', 'In Progress', 'Completed', 'Closed'],
        datasets: [
          {
            data: [
              s.workOrdersOpenCount,
              s.workOrdersInProgressCount,
              s.workOrdersCompletedCount,
              s.workOrdersClosedCount,
            ],
            backgroundColor: [
              CHART_COLORS.open,
              CHART_COLORS.inProgress,
              CHART_COLORS.completed,
              CHART_COLORS.closed,
            ],
            borderWidth: 0,
            hoverOffset: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '62%',
        plugins: {
          legend: { position: 'bottom', labels: { usePointStyle: true, boxWidth: 8, padding: 16 } },
        },
      },
    };
  });

  /** Bar — incident pipeline. */
  readonly incidentChart = computed<ChartConfiguration | null>(() => {
    const s = this.summary();
    if (!s) {
      return null;
    }
    return {
      type: 'bar',
      data: {
        labels: ['Pending Inspection', 'Inspection Done', 'Vendor Assigned', 'Pending Verification'],
        datasets: [
          {
            label: 'Incidents',
            data: [
              s.pendingInspectionCount,
              s.inspectionCompletedCount,
              s.vendorAssignedCount,
              s.pendingVerificationCount,
            ],
            backgroundColor: [
              CHART_COLORS.pendingInspection,
              CHART_COLORS.inspectionDone,
              CHART_COLORS.vendorAssigned,
              CHART_COLORS.pendingVerification,
            ],
            borderRadius: 8,
            maxBarThickness: 56,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false } },
          y: { beginAtZero: true, ticks: { precision: 0 }, grid: { color: 'rgba(13,27,75,0.06)' } },
        },
      },
    };
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load the dashboard.');
        this.loading.set(false);
      },
    });
  }
}
