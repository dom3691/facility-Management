import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { VendorInput, VendorService } from '../../../vendors/vendor.service';
import {
  categoryNameToInt,
  Vendor,
  VENDOR_CATEGORY_INT_OPTIONS,
} from '../../../vendors/vendor.models';

export interface VendorFormData {
  vendor?: Vendor;
}

/** Create/edit a vendor. Closes with the saved {@link Vendor}. */
@Component({
  selector: 'app-vendor-form-dialog',
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
  templateUrl: './vendor-form-dialog.html',
  styleUrl: './vendor-form-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(VendorService);
  private readonly dialogRef = inject<MatDialogRef<VendorFormDialog, Vendor>>(MatDialogRef);
  readonly data = inject<VendorFormData>(MAT_DIALOG_DATA);

  readonly categories = VENDOR_CATEGORY_INT_OPTIONS;
  readonly isEdit = !!this.data.vendor;
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    vendorName: [this.data.vendor?.vendorName ?? '', [Validators.required, Validators.maxLength(200)]],
    vendorCategory: [categoryNameToInt(this.data.vendor?.vendorCategory) ?? (null as number | null), Validators.required],
    contactPerson: [this.data.vendor?.contactPerson ?? ''],
    email: [this.data.vendor?.email ?? '', [Validators.email]],
    phoneNumber: [this.data.vendor?.phoneNumber ?? ''],
    isActive: [this.data.vendor?.isActive ?? true],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const payload: VendorInput = {
      vendorName: value.vendorName.trim(),
      vendorCategory: value.vendorCategory as number,
      contactPerson: value.contactPerson.trim() || null,
      email: value.email.trim() || null,
      phoneNumber: value.phoneNumber.trim() || null,
      isActive: value.isActive,
    };

    this.saving.set(true);
    this.error.set(null);

    const request$ = this.isEdit
      ? this.service.update(this.data.vendor!.id, payload)
      : this.service.create(payload);

    request$.subscribe({
      next: (vendor) => {
        this.saving.set(false);
        this.dialogRef.close(vendor);
      },
      error: (err: NormalizedHttpError) => {
        this.saving.set(false);
        this.error.set(err?.message ?? 'Could not save the vendor.');
      },
    });
  }
}
