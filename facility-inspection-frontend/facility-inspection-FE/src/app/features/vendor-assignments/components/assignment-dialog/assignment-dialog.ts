import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { VendorService } from '../../../vendors/vendor.service';
import { Vendor, VENDOR_CATEGORIES } from '../../../vendors/vendor.models';
import { VendorAssignmentService } from '../../vendor-assignment.service';
import { VendorAssignment } from '../../vendor-assignment.models';

export interface AssignmentDialogData {
  incidentId: string;
  inspectionId: string;
  incidentNumber?: string | null;
}

/** Material dialog to assign a vendor: category → vendor (filtered) → notes. */
@Component({
  selector: 'app-assignment-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './assignment-dialog.html',
  styleUrl: './assignment-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssignmentDialog {
  private readonly fb = inject(FormBuilder);
  private readonly vendors = inject(VendorService);
  private readonly assignments = inject(VendorAssignmentService);
  private readonly dialogRef = inject<MatDialogRef<AssignmentDialog, VendorAssignment>>(MatDialogRef);
  readonly data = inject<AssignmentDialogData>(MAT_DIALOG_DATA);

  readonly categories = VENDOR_CATEGORIES;
  readonly vendorList = signal<Vendor[]>([]);
  readonly loadingVendors = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    vendorCategory: ['', Validators.required],
    vendorId: [{ value: '', disabled: true }, Validators.required],
    notes: [''],
  });

  constructor() {
    this.form.controls.vendorCategory.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((category) => this.onCategoryChange(category));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);

    this.assignments
      .create({
        incidentId: this.data.incidentId,
        inspectionId: this.data.inspectionId,
        vendorId: value.vendorId,
        vendorCategory: value.vendorCategory,
        notes: value.notes.trim() || undefined,
      })
      .subscribe({
        next: (created) => {
          this.saving.set(false);
          this.dialogRef.close(created);
        },
        error: (err: NormalizedHttpError) => {
          this.saving.set(false);
          this.error.set(err?.message ?? 'Could not assign the vendor.');
        },
      });
  }

  private onCategoryChange(category: string): void {
    const vendorCtrl = this.form.controls.vendorId;
    vendorCtrl.reset('');
    vendorCtrl.disable();
    this.vendorList.set([]);

    if (!category) {
      return;
    }
    this.loadingVendors.set(true);
    this.vendors.list(1, 100, category, true).subscribe({
      next: (result) => {
        this.vendorList.set(result.items);
        vendorCtrl.enable();
        this.loadingVendors.set(false);
      },
      error: () => {
        this.error.set('Could not load vendors for this category.');
        this.loadingVendors.set(false);
      },
    });
  }
}
