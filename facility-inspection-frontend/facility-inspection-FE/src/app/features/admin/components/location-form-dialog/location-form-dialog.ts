import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReferenceService } from '../../../../core/services/reference.service';
import { Facility, Location } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';

export interface LocationFormData {
  facilities: Facility[];
  facilityId?: string;
}

/** Create a location under a facility. Closes with the saved {@link Location}. */
@Component({
  selector: 'app-location-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>Add Location</h2>
    <mat-dialog-content>
      @if (error(); as err) { <div class="alert">{{ err }}</div> }
      <form [formGroup]="form" class="form" novalidate>
        <mat-form-field appearance="outline">
          <mat-label>Facility</mat-label>
          <mat-select formControlName="facilityId">
            @for (f of data.facilities; track f.id) {
              <mat-option [value]="f.id">{{ f.name }}</mat-option>
            }
          </mat-select>
          @if (form.controls.facilityId.touched && form.controls.facilityId.hasError('required')) {
            <mat-error>Facility is required.</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
          @if (form.controls.name.touched && form.controls.name.hasError('required')) {
            <mat-error>Name is required.</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Code (optional)</mat-label>
          <input matInput formControlName="code" />
        </mat-form-field>
        <mat-slide-toggle formControlName="isActive" color="primary">Active</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close [disabled]="saving()">Cancel</button>
      <button mat-flat-button class="save-btn" (click)="submit()" [disabled]="saving()">
        @if (saving()) { <mat-spinner diameter="18" /> } @else { Create Location }
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .form { display: flex; flex-direction: column; min-width: 340px; }
      mat-form-field { width: 100%; }
      .alert { margin-bottom: 1rem; padding: 0.6rem 0.8rem; border-radius: 10px; background: #fdecec; color: #b3261e; font-size: 0.83rem; }
      .save-btn { --mat-sys-primary: var(--brand-navy); }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LocationFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly reference = inject(ReferenceService);
  private readonly dialogRef = inject<MatDialogRef<LocationFormDialog, Location>>(MatDialogRef);
  readonly data = inject<LocationFormData>(MAT_DIALOG_DATA);

  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    facilityId: [this.data.facilityId ?? '', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    code: [''],
    isActive: [true],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    this.reference
      .createLocation({
        facilityId: value.facilityId,
        name: value.name.trim(),
        code: value.code.trim() || undefined,
        isActive: value.isActive,
      })
      .subscribe({
        next: (location) => {
          this.saving.set(false);
          this.dialogRef.close(location);
        },
        error: (err: NormalizedHttpError) => {
          this.saving.set(false);
          this.error.set(err?.message ?? 'Could not create the location.');
        },
      });
  }
}
