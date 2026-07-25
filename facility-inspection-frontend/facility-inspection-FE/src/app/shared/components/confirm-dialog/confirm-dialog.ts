import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  danger?: boolean;
}

/** Small reusable confirmation dialog. Closes with `true` when confirmed. */
@Component({
  selector: 'app-confirm-dialog',
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p class="msg">{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button
        mat-flat-button
        [mat-dialog-close]="true"
        class="confirm"
        [class.confirm--danger]="data.danger"
      >
        {{ data.confirmLabel || 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .msg {
        margin: 0;
        color: var(--brand-navy);
        font-size: 0.9rem;
        line-height: 1.5;
        max-width: 360px;
      }
      .confirm {
        --mat-sys-primary: var(--brand-navy);
      }
      .confirm--danger {
        --mat-sys-primary: #dc2626;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
}
