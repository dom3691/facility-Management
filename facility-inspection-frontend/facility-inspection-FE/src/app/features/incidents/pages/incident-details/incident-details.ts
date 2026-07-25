import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { FileService } from '../../../../core/services/file.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { IncidentService } from '../../incident.service';
import { IncidentAttachment, IncidentDetail } from '../../incident.models';

@Component({
  selector: 'app-incident-details',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
  ],
  templateUrl: './incident-details.html',
  styleUrl: './incident-details.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IncidentDetails {
  private readonly service = inject(IncidentService);
  private readonly fileService = inject(FileService);
  private readonly snackbar = inject(MatSnackBar);

  /** Route param, bound via `withComponentInputBinding()`. */
  readonly id = input.required<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly incident = signal<IncidentDetail | null>(null);
  readonly downloadingId = signal<string | null>(null);

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
      next: (incident) => {
        this.incident.set(incident);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(
          err?.status === 404 ? 'Incident not found.' : (err?.message ?? 'Could not load incident.'),
        );
        this.loading.set(false);
      },
    });
  }

  download(attachment: IncidentAttachment): void {
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

  formatSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(0)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
