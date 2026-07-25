import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { IncidentService } from '../../../incidents/incident.service';
import { IncidentDetail } from '../../../incidents/incident.models';
import { InspectionService } from '../../../inspections/inspection.service';
import { Inspection } from '../../../inspections/inspection.models';
import { vendorCategoryLabel } from '../../../vendors/vendor.models';
import { VendorAssignmentService } from '../../vendor-assignment.service';
import { VendorAssignment } from '../../vendor-assignment.models';
import { AssignmentDialog, AssignmentDialogData } from '../../components/assignment-dialog/assignment-dialog';

@Component({
  selector: 'app-vendor-assignment',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './vendor-assignment.html',
  styleUrl: './vendor-assignment.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorAssignmentComponent {
  private readonly incidents = inject(IncidentService);
  private readonly inspections = inject(InspectionService);
  private readonly assignmentsApi = inject(VendorAssignmentService);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  readonly incidentId = input.required<string>();

  readonly historyColumns = ['vendor', 'category', 'assigned', 'notes'];
  readonly categoryLabel = vendorCategoryLabel;

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly incident = signal<IncidentDetail | null>(null);
  readonly inspectionList = signal<Inspection[]>([]);
  readonly assignments = signal<VendorAssignment[]>([]);

  /** The inspection this assignment attaches to (a fault requiring a vendor). */
  readonly eligibleInspection = computed<Inspection | null>(() => {
    const list = this.inspectionList();
    return list.find((i) => i.requiresVendor) ?? list[0] ?? null;
  });

  readonly canAssign = computed(
    () => !!this.eligibleInspection() && this.incident()?.status === 'AwaitingVendorAssignment',
  );

  constructor() {
    effect(() => {
      const id = this.incidentId();
      if (id) {
        this.load(id);
      }
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.incidents.getById(id).subscribe({
      next: (incident) => {
        this.incident.set(incident);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load the incident.');
        this.loading.set(false);
      },
    });

    this.inspections.getByIncident(id).subscribe({
      next: (list) => this.inspectionList.set(list),
      error: () => this.inspectionList.set([]),
    });

    this.loadAssignments(id);
  }

  loadAssignments(id: string): void {
    this.assignmentsApi.getByIncident(id).subscribe({
      next: (list) => this.assignments.set(list),
      error: () => this.assignments.set([]),
    });
  }

  openAssignDialog(): void {
    const inspection = this.eligibleInspection();
    const incident = this.incident();
    if (!inspection || !incident) {
      return;
    }

    const data: AssignmentDialogData = {
      incidentId: incident.id,
      inspectionId: inspection.id,
      incidentNumber: incident.incidentNumber,
    };

    this.dialog
      .open(AssignmentDialog, { data, width: '460px', autoFocus: false })
      .afterClosed()
      .subscribe((result?: VendorAssignment) => {
        if (result) {
          this.snackbar.open(`Assigned to ${result.vendorName ?? 'vendor'}.`, 'Dismiss', {
            duration: 4000,
          });
          // Refresh incident (status changes to VendorAssigned) and history.
          this.load(this.incidentId());
        }
      });
  }
}
