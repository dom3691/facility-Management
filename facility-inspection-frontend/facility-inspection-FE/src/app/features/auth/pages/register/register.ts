import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { RegisterRequest } from '../../../../core/models';

/** Backend password policy: ≥8 chars incl. upper, lower, digit and a symbol. */
const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

/** Cross-field validator: password === confirmPassword. */
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

/**
 * Registration page. Public sign-up always creates an Initiator (the backend
 * ignores any role), so no role selector is shown. On success the response
 * carries a token, so the user is signed in and redirected automatically.
 */
@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly hide = signal(true);
  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      sapId: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  constructor() {
    if (this.auth.isAuthenticated()) {
      void this.router.navigateByUrl(this.auth.postAuthRoute());
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.serverError.set(null);

    const value = this.form.getRawValue();
    const payload: RegisterRequest = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      sapId: value.sapId.trim(),
      email: value.email.trim(),
      phoneNumber: value.phoneNumber.trim() || undefined,
      password: value.password,
    };

    this.auth.register(payload).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl(this.auth.landingRoute());
      },
      error: (err: NormalizedHttpError) => {
        this.loading.set(false);
        this.applyServerErrors(err);
      },
    });
  }

  /** Maps backend field errors (PascalCase keys) onto the matching controls. */
  private applyServerErrors(err: NormalizedHttpError): void {
    this.serverError.set(err?.message ?? 'Registration failed. Please try again.');
    for (const [field, messages] of Object.entries(err?.fieldErrors ?? {})) {
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      const control = this.form.get(key);
      if (control) {
        control.setErrors({ ...(control.errors ?? {}), server: messages.join(' ') });
      }
    }
  }
}
