import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ReferenceService } from '../../../../core/services/reference.service';
import { Facility, Location } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { FileDrop } from '../../../../shared/components/file-drop/file-drop';
import { IncidentService } from '../../incident.service';
import { BUSINESS_UNITS, CreateIncidentInput } from '../../incident.models';

@Component({
  selector: 'app-create-incident',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
    FileDrop,
  ],
  templateUrl: './create-incident.html',
  styleUrl: './create-incident.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateIncident {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(IncidentService);
  private readonly reference = inject(ReferenceService);
  private readonly router = inject(Router);
  private readonly snackbar = inject(MatSnackBar);

  readonly businessUnits = BUSINESS_UNITS;
  readonly facilities = signal<Facility[]>([]);
  readonly locations = signal<Location[]>([]);
  readonly attachments = signal<File[]>([]);

  readonly loading = signal(false);
  readonly loadingLocations = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly maxDate = new Date();

  readonly form = this.fb.group({
    businessUnit: this.fb.nonNullable.control('', Validators.required),
    sapId: this.fb.nonNullable.control('', Validators.required),
    facilityId: this.fb.nonNullable.control('', Validators.required),
    locationId: this.fb.nonNullable.control({ value: '', disabled: true }, Validators.required),
    incidentDate: this.fb.control<Date | null>(null, Validators.required),
    description: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(2000)]),
  });

  constructor() {
    this.reference.getFacilities().subscribe({
      next: (facilities) => this.facilities.set(facilities),
      error: () => this.serverError.set('Could not load facilities.'),
    });

    // Cascade: when the facility changes, load its locations and reset selection.
    this.form.controls.facilityId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((facilityId) => this.onFacilityChange(facilityId));
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
    const payload: CreateIncidentInput = {
      businessUnit: value.businessUnit,
      sapId: value.sapId.trim(),
      facilityId: value.facilityId,
      locationId: value.locationId,
      incidentDate: (value.incidentDate as Date).toISOString(),
      description: value.description.trim(),
      attachments: this.attachments(),
    };

    this.loading.set(true);
    this.serverError.set(null);

    this.service.create(payload).subscribe({
      next: (created) => {
        this.loading.set(false);
        this.snackbar.open(`Incident ${created.incidentNumber} reported.`, 'Dismiss', {
          duration: 4000,
        });
        void this.router.navigate(['/incidents', created.id]);
      },
      error: (err: NormalizedHttpError) => {
        this.loading.set(false);
        this.applyServerErrors(err);
      },
    });
  }

  private onFacilityChange(facilityId: string): void {
    const locationCtrl = this.form.controls.locationId;
    locationCtrl.reset('');
    locationCtrl.disable();
    this.locations.set([]);

    if (!facilityId) {
      return;
    }
    this.loadingLocations.set(true);
    this.reference.getLocations(facilityId).subscribe({
      next: (locations) => {
        this.locations.set(locations);
        locationCtrl.enable();
        this.loadingLocations.set(false);
      },
      error: () => {
        this.serverError.set('Could not load locations for this facility.');
        this.loadingLocations.set(false);
      },
    });
  }

  private applyServerErrors(err: NormalizedHttpError): void {
    this.serverError.set(err?.message ?? 'Could not report the incident.');
    for (const [field, messages] of Object.entries(err?.fieldErrors ?? {})) {
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      const control = this.form.get(key);
      control?.setErrors({ ...(control.errors ?? {}), server: messages.join(' ') });
    }
  }
}
