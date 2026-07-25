import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { VerificationService } from '../../verification.service';
import { Verification } from '../../verification.models';

export interface VerificationFormData {
  workOrderId: string;
  workOrderNumber?: string | null;
}

/** Material dialog: approve (Fixed) or reject (Not Fixed) completed work. */
@Component({
  selector: 'app-verification-form',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './verification-form.html',
  styleUrl: './verification-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerificationForm {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(VerificationService);
  private readonly dialogRef =
    inject<MatDialogRef<VerificationForm, Verification>>(MatDialogRef);
  readonly data = inject<VerificationFormData>(MAT_DIALOG_DATA);

  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    decision: ['', Validators.required],
    comments: ['', Validators.maxLength(1000)],
  });

  readonly decision = signal<string>('');

  readonly confirmLabel = computed(() => {
    switch (this.decision()) {
      case 'Fixed':
        return 'Approve & Close';
      case 'NotFixed':
        return 'Reject & Reopen';
      default:
        return 'Confirm Decision';
    }
  });

  select(decision: string): void {
    this.form.controls.decision.setValue(decision);
    this.decision.set(decision);

    // Comments are mandatory only when rejecting — set the validator on the control
    // itself so Material renders the error under the field.
    const comments = this.form.controls.comments;
    comments.setValidators(
      decision === 'NotFixed'
        ? [Validators.required, Validators.maxLength(1000)]
        : [Validators.maxLength(1000)],
    );
    comments.updateValueAndValidity();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);

    this.service
      .create({
        workOrderId: this.data.workOrderId,
        decision: value.decision,
        comments: value.comments.trim() || undefined,
      })
      .subscribe({
        next: (result) => {
          this.saving.set(false);
          this.dialogRef.close(result);
        },
        error: (err: NormalizedHttpError) => {
          this.saving.set(false);
          this.error.set(err?.message ?? 'Could not record the verification.');
        },
      });
  }
}
