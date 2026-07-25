import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AdminCreateUserRequest, ALL_ROLES, AppRole } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { VendorService } from '../../../vendors/vendor.service';
import { Vendor } from '../../../vendors/vendor.models';
import { AdminUserService } from '../../admin-user.service';

/** Provision new users. Password is auto-generated; an invitation email is sent. */
@Component({
  selector: 'app-admin-users',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeader,
  ],
  templateUrl: './users.html',
  styleUrl: './users.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUsers implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AdminUserService);
  private readonly vendorService = inject(VendorService);
  private readonly snackbar = inject(MatSnackBar);

  readonly roles = ALL_ROLES;
  readonly AppRole = AppRole;
  readonly saving = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly vendors = signal<Vendor[]>([]);
  readonly vendorsLoading = signal(true);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    sapId: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    role: ['', Validators.required],
    vendorId: [''],
  });

  readonly isVendorRole = computed(() => this.form.controls.role.value === AppRole.Vendor);

  ngOnInit(): void {
    this.vendorService.list(1, 100, null, true).subscribe({
      next: (result) => {
        this.vendors.set(result.items);
        this.vendorsLoading.set(false);
      },
      error: () => this.vendorsLoading.set(false),
    });
  }

  submit(): void {
    if (this.isVendorRole() && !this.form.controls.vendorId.value) {
      this.form.controls.vendorId.setErrors({ required: true });
      this.form.controls.vendorId.markAsTouched();
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: AdminCreateUserRequest = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      sapId: value.sapId.trim(),
      email: value.email.trim(),
      phoneNumber: value.phoneNumber.trim() || undefined,
      role: value.role as AppRole,
      vendorId: this.isVendorRole() ? value.vendorId : undefined,
    };

    this.saving.set(true);
    this.serverError.set(null);

    this.service.create(payload).subscribe({
      next: (created) => {
        this.saving.set(false);
        const inviteNote = created.invitationEmailSent
          ? ' An invitation email has been sent.'
          : ' Invitation email could not be confirmed — use Forgot password if needed.';
        this.snackbar.open(`User ${created.email} created (${created.roles.join(', ')}).${inviteNote}`, 'Dismiss', {
          duration: 6000,
        });
        this.form.reset({ role: '', vendorId: '' });
      },
      error: (err: NormalizedHttpError) => {
        this.saving.set(false);
        this.serverError.set(err?.message ?? 'Could not create the user.');
      },
    });
  }
}
