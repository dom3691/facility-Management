import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { FileDrop } from '../../../../shared/components/file-drop/file-drop';
import { IncidentService } from '../../../incidents/incident.service';
import { IncidentDetail } from '../../../incidents/incident.models';
import { InspectionService } from '../../inspection.service';
import { CreateInspectionInput, INSPECTION_CLASSIFICATIONS } from '../../inspection.models';

@Component({
  selector: 'app-inspection-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    FileDrop,
  ],
  templateUrl: './inspection-form.html',
  styleUrl: './inspection-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InspectionForm {
  private readonly fb = inject(FormBuilder);
  private readonly incidents = inject(IncidentService);
  private readonly inspections = inject(InspectionService);
  private readonly router = inject(Router);
  private readonly snackbar = inject(MatSnackBar);

  /** Route param `/inspections/new/:incidentId`. */
  readonly incidentId = input.required<string>();

  readonly classifications = INSPECTION_CLASSIFICATIONS;
  readonly incident = signal<IncidentDetail | null>(null);
  readonly attachments = signal<File[]>([]);

  readonly loadingIncident = signal(true);
  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    classification: ['', Validators.required],
    comments: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  constructor() {
    effect(() => {
      const id = this.incidentId();
      if (id) {
        this.loadIncident(id);
      }
    });
  }

  onFilesChange(files: File[]): void {
    this.attachments.set(files);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: CreateInspectionInput = {
      incidentId: this.incidentId(),
      classification: value.classification,
      comments: value.comments.trim(),
      attachments: this.attachments(),
    };

    this.loading.set(true);
    this.serverError.set(null);

    this.inspections.create(payload).subscribe({
      next: (created) => {
        this.loading.set(false);
        this.snackbar.open('Inspection recorded.', 'Dismiss', { duration: 4000 });
        void this.router.navigate(['/inspections', created.id]);
      },
      error: (err: NormalizedHttpError) => {
        this.loading.set(false);
        this.serverError.set(err?.message ?? 'Could not record the inspection.');
      },
    });
  }

  private loadIncident(id: string): void {
    this.loadingIncident.set(true);
    this.incidents.getById(id).subscribe({
      next: (incident) => {
        this.incident.set(incident);
        this.loadingIncident.set(false);
      },
      error: () => {
        this.serverError.set('Could not load the incident to inspect.');
        this.loadingIncident.set(false);
      },
    });
  }
}
