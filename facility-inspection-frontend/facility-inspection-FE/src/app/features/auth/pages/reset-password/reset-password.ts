import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
  selector: 'app-reset-password',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPassword {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly hide = signal(true);
  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  submit(): void {
    if (!this.email || !this.token) {
      this.serverError.set('This reset link is invalid or incomplete. Request a new one.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.serverError.set(null);

    const { newPassword } = this.form.getRawValue();
    this.auth
      .resetPassword({ email: this.email, token: this.token, newPassword })
      .subscribe({
        next: () => {
          this.loading.set(false);
          void this.router.navigate(['/auth/login'], {
            queryParams: { message: 'Password updated. You can now sign in.' },
          });
        },
        error: (err: NormalizedHttpError) => {
          this.loading.set(false);
          this.serverError.set(err?.message ?? 'Unable to reset password. The link may have expired.');
        },
      });
  }
}
