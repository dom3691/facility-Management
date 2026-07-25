import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { FileService } from '../../../../core/services/file.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusChip } from '../../../../shared/components/status-chip/status-chip';
import { FileDrop } from '../../../../shared/components/file-drop/file-drop';
import { WorkOrderService } from '../../../work-orders/work-order.service';
import { WorkOrder } from '../../../work-orders/work-order.models';
import { VendorUpdateService } from '../../vendor-update.service';
import { VendorUpdate, VendorUpdateAttachment } from '../../vendor-update.models';

const ACTIONABLE = ['Assigned', 'Open', 'InProgress'];

@Component({
  selector: 'app-vendor-update',
  imports: [
    DatePipe,
    LowerCasePipe,
    ReactiveFormsModule,
    RouterLink,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    PageHeader,
    StatusChip,
    FileDrop,
  ],
  templateUrl: './vendor-update.html',
  styleUrl: './vendor-update.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendorUpdateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly workOrders = inject(WorkOrderService);
  private readonly updates = inject(VendorUpdateService);
  private readonly fileService = inject(FileService);
  private readonly snackbar = inject(MatSnackBar);

  readonly workOrderId = input.required<string>();

  private readonly progressDrop = viewChild<FileDrop>('progressDrop');
  private readonly completeDrop = viewChild<FileDrop>('completeDrop');

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly workOrder = signal<WorkOrder | null>(null);
  readonly history = signal<VendorUpdate[]>([]);

  readonly progressFiles = signal<File[]>([]);
  readonly completeFiles = signal<File[]>([]);
  readonly savingProgress = signal(false);
  readonly savingComplete = signal(false);
  readonly downloadingId = signal<string | null>(null);

  readonly canPost = computed(() => {
    const wo = this.workOrder();
    return !!wo && ACTIONABLE.includes(wo.status);
  });

  readonly progressForm = this.fb.nonNullable.group({
    progressPercentage: [50, [Validators.required, Validators.min(0), Validators.max(100)]],
    progressComment: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  readonly completeForm = this.fb.nonNullable.group({
    completionComment: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  constructor() {
    effect(() => {
      const id = this.workOrderId();
      if (id) {
        this.load(id);
      }
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.workOrders.getById(id).subscribe({
      next: (wo) => {
        this.workOrder.set(wo);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load work order.');
        this.loading.set(false);
      },
    });

    this.updates.getByWorkOrder(id).subscribe({
      next: (list) => this.history.set(list),
      error: () => this.history.set([]),
    });
  }

  submitProgress(): void {
    if (this.progressForm.invalid) {
      this.progressForm.markAllAsTouched();
      return;
    }
    const value = this.progressForm.getRawValue();
    this.savingProgress.set(true);

    this.updates
      .createProgress({
        workOrderId: this.workOrderId(),
        progressComment: value.progressComment.trim(),
        progressPercentage: value.progressPercentage,
        attachments: this.progressFiles(),
      })
      .subscribe({
        next: () => {
          this.savingProgress.set(false);
          this.snackbar.open('Progress update posted.', 'Dismiss', { duration: 3500 });
          this.progressForm.reset({ progressPercentage: 50, progressComment: '' });
          this.progressDrop()?.reset();
          this.load(this.workOrderId());
        },
        error: (err: NormalizedHttpError) => {
          this.savingProgress.set(false);
          this.snackbar.open(err?.message ?? 'Could not post the update.', 'Dismiss', { duration: 4000 });
        },
      });
  }

  submitComplete(): void {
    if (this.completeForm.invalid) {
      this.completeForm.markAllAsTouched();
      return;
    }
    const value = this.completeForm.getRawValue();
    this.savingComplete.set(true);

    this.updates
      .markComplete({
        workOrderId: this.workOrderId(),
        completionComment: value.completionComment.trim(),
        attachments: this.completeFiles(),
      })
      .subscribe({
        next: () => {
          this.savingComplete.set(false);
          this.snackbar.open('Work order marked complete.', 'Dismiss', { duration: 4000 });
          this.completeForm.reset({ completionComment: '' });
          this.completeDrop()?.reset();
          this.load(this.workOrderId());
        },
        error: (err: NormalizedHttpError) => {
          this.savingComplete.set(false);
          this.snackbar.open(err?.message ?? 'Could not complete the work order.', 'Dismiss', { duration: 4000 });
        },
      });
  }

  download(attachment: VendorUpdateAttachment): void {
    this.downloadingId.set(attachment.id);
    this.fileService.download(attachment.id).subscribe({
      next: (blob) => {
        this.fileService.saveBlob(blob, attachment.fileName);
        this.downloadingId.set(null);
      },
      error: () => {
        this.downloadingId.set(null);
        this.snackbar.open('Could not download the file.', 'Dismiss', { duration: 3000 });
      },
    });
  }
}
