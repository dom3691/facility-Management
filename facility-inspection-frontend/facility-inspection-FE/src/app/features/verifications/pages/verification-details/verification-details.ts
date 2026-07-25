import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { FileService } from '../../../../core/services/file.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { WorkOrderService } from '../../../work-orders/work-order.service';
import { WorkOrder } from '../../../work-orders/work-order.models';
import { IncidentService } from '../../../incidents/incident.service';
import { IncidentDetail } from '../../../incidents/incident.models';
import { InspectionService } from '../../../inspections/inspection.service';
import { Inspection } from '../../../inspections/inspection.models';
import { VendorUpdateService } from '../../../vendor-updates/vendor-update.service';
import { VendorUpdate, VendorUpdateAttachment } from '../../../vendor-updates/vendor-update.models';
import { VerificationService } from '../../verification.service';
import { Verification } from '../../verification.models';
import { VerificationForm, VerificationFormData } from '../../components/verification-form/verification-form';

@Component({
  selector: 'app-verification-details',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './verification-details.html',
  styleUrl: './verification-details.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerificationDetails {
  private readonly workOrders = inject(WorkOrderService);
  private readonly incidents = inject(IncidentService);
  private readonly inspections = inject(InspectionService);
  private readonly vendorUpdatesApi = inject(VendorUpdateService);
  private readonly verifications = inject(VerificationService);
  private readonly fileService = inject(FileService);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);
  private readonly router = inject(Router);

  /** Route param `/verifications/:workOrderId`. */
  readonly workOrderId = input.required<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly workOrder = signal<WorkOrder | null>(null);
  readonly incident = signal<IncidentDetail | null>(null);
  readonly inspection = signal<Inspection | null>(null);
  readonly vendorUpdates = signal<VendorUpdate[]>([]);
  readonly evidenceRestricted = signal(false);
  readonly history = signal<Verification[]>([]);
  readonly downloadingId = signal<string | null>(null);

  readonly canVerify = computed(() => this.workOrder()?.status === 'Completed');

  constructor() {
    effect(() => {
      const id = this.workOrderId();
      if (id) {
        this.load(id);
      }
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.workOrders.getById(id).subscribe({
      next: (wo) => {
        this.workOrder.set(wo);
        this.loading.set(false);
        this.loadIncidentContext(wo.incidentId);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load the work order.');
        this.loading.set(false);
      },
    });

    // Vendor evidence — restricted to Vendor/Admin on the backend, so degrade gracefully.
    this.vendorUpdatesApi.getByWorkOrder(id).subscribe({
      next: (list) => this.vendorUpdates.set(list),
      error: (err: NormalizedHttpError) => {
        this.evidenceRestricted.set(err?.status === 403);
        this.vendorUpdates.set([]);
      },
    });

    this.verifications.getByWorkOrder(id).subscribe({
      next: (list) => this.history.set(list),
      error: () => this.history.set([]),
    });
  }

  openVerifyDialog(): void {
    const wo = this.workOrder();
    if (!wo) {
      return;
    }
    const data: VerificationFormData = { workOrderId: wo.id, workOrderNumber: wo.workOrderNumber };

    this.dialog
      .open(VerificationForm, { data, width: '520px', autoFocus: false })
      .afterClosed()
      .subscribe((result?: Verification) => {
        if (result) {
          const fixed = result.decision === 'Fixed';
          this.snackbar.open(
            fixed ? 'Verified as Fixed — incident closed.' : 'Marked Not Fixed — reopened for the vendor.',
            'Dismiss',
            { duration: 4500 },
          );
          void this.router.navigate(['/verifications']);
        }
      });
  }

  download(attachment: VendorUpdateAttachment): void {
    this.downloadingId.set(attachment.id);
    this.fileService.download(attachment.id).subscribe({
      next: (blob) => {
        this.fileService.saveBlob(blob, attachment.fileName);
        this.downloadingId.set(null);
      },
      error: () => {
        this.downloadingId.set(null);
        this.snackbar.open('Could not download the file.', 'Dismiss', { duration: 3000 });
      },
    });
  }

  private loadIncidentContext(incidentId: string): void {
    this.incidents.getById(incidentId).subscribe({
      next: (inc) => this.incident.set(inc),
      error: () => this.incident.set(null),
    });

    this.inspections.getByIncident(incidentId).subscribe({
      next: (list) => this.inspection.set(list.find((i) => i.requiresVendor) ?? list[0] ?? null),
      error: () => this.inspection.set(null),
    });
  }
}
