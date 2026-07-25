import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AuthService } from '../../../../core/services/auth.service';
import { FileService } from '../../../../core/services/file.service';
import { AuditService } from '../../../../core/services/audit.service';
import { AppRole, AuditLogEntry } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { StatusTimeline } from '../../../../shared/components/status-timeline/status-timeline';
import { InspectionService } from '../../inspection.service';
import { Inspection, InspectionAttachment } from '../../inspection.models';

@Component({
  selector: 'app-inspection-details',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
    StatusTimeline,
  ],
  templateUrl: './inspection-details.html',
  styleUrl: './inspection-details.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InspectionDetails {
  private readonly service = inject(InspectionService);
  private readonly fileService = inject(FileService);
  private readonly auditService = inject(AuditService);
  private readonly auth = inject(AuthService);
  private readonly snackbar = inject(MatSnackBar);

  readonly id = input.required<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly inspection = signal<Inspection | null>(null);
  readonly downloadingId = signal<string | null>(null);

  // Audit trail (Admin-only endpoint).
  readonly isAdmin = this.auth.hasRole(AppRole.Admin);
  readonly auditLoading = signal(false);
  readonly auditEntries = signal<AuditLogEntry[]>([]);

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
      next: (inspection) => {
        this.inspection.set(inspection);
        this.loading.set(false);
        if (this.isAdmin) {
          this.loadAudit(inspection.incidentId);
        }
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(
          err?.status === 404 ? 'Inspection not found.' : (err?.message ?? 'Could not load inspection.'),
        );
        this.loading.set(false);
      },
    });
  }

  download(attachment: InspectionAttachment): void {
    this.downloadingId.set(attachment.id);
    this.fileService.download(attachment.id).subscribe({
      next: (blob) => {
        this.fileService.saveBlob(blob, attachment.fileName);
        this.downloadingId.set(null);
      },
      error: () => {
        this.downloadingId.set(null);
        this.snackbar.open('Could not download the attachment.', 'Dismiss', { duration: 3000 });
      },
    });
  }

  /** Humanises an audit action code, e.g. "InspectionCompleted" → "Inspection Completed". */
  humanizeAction(action: string): string {
    return action.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(0)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private loadAudit(incidentId: string): void {
    this.auditLoading.set(true);
    this.auditService.getByEntity('Incident', incidentId).subscribe({
      next: (entries) => {
        this.auditEntries.set(entries);
        this.auditLoading.set(false);
      },
      error: () => this.auditLoading.set(false), // non-fatal; section shows empty
    });
  }
}
