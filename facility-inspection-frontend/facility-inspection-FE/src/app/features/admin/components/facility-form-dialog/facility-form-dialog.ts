import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReferenceService } from '../../../../core/services/reference.service';
import { Facility } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';

/** Create a facility. Closes with the saved {@link Facility}. */
@Component({
  selector: 'app-facility-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>Add Facility</h2>
    <mat-dialog-content>
      @if (error(); as err) { <div class="alert">{{ err }}</div> }
      <form [formGroup]="form" class="form" novalidate>
        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
          @if (form.controls.name.touched && form.controls.name.hasError('required')) {
            <mat-error>Name is required.</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Code</mat-label>
          <input matInput formControlName="code" placeholder="e.g. HO" />
          @if (form.controls.code.touched && form.controls.code.hasError('required')) {
            <mat-error>Code is required.</mat-error>
          }
        </mat-form-field>
        <mat-slide-toggle formControlName="isActive" color="primary">Active</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close [disabled]="saving()">Cancel</button>
      <button mat-flat-button class="save-btn" (click)="submit()" [disabled]="saving()">
        @if (saving()) { <mat-spinner diameter="18" /> } @else { Create Facility }
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
export class FacilityFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly reference = inject(ReferenceService);
  private readonly dialogRef = inject<MatDialogRef<FacilityFormDialog, Facility>>(MatDialogRef);

  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    code: ['', [Validators.required, Validators.maxLength(20)]],
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
      .createFacility({ name: value.name.trim(), code: value.code.trim(), isActive: value.isActive })
      .subscribe({
        next: (facility) => {
          this.saving.set(false);
          this.dialogRef.close(facility);
        },
        error: (err: NormalizedHttpError) => {
          this.saving.set(false);
          this.error.set(err?.message ?? 'Could not create the facility.');
        },
      });
  }
}
