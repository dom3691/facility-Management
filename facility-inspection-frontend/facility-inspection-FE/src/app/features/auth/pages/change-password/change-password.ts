import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-change-password',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './change-password.html',
  styleUrl: './change-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePassword {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly hide = signal(true);
  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly mustChange = this.auth.requiresPasswordChange;

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.serverError.set(null);

    const { currentPassword, newPassword } = this.form.getRawValue();
    this.auth.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl(this.auth.postAuthRoute());
      },
      error: (err: NormalizedHttpError) => {
        this.loading.set(false);
        this.serverError.set(err?.message ?? 'Unable to change password.');
      },
    });
  }

  signOut(): void {
    this.auth.logout();
  }
}
