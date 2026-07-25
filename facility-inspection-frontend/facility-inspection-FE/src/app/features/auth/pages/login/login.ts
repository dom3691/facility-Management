import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';

/** Sign-in page. Reactive form + Material fields; stores the session and redirects by role. */
@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly hide = signal(true);
  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    remember: [true],
  });

  constructor() {
    // Already signed in? Skip the form.
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

    const { email, password, remember } = this.form.getRawValue();
    this.auth.login({ email, password }, remember).subscribe({
      next: () => this.redirect(),
      error: (err: NormalizedHttpError) => {
        this.loading.set(false);
        this.serverError.set(
          err?.status === 401
            ? 'Invalid email or password.'
            : (err?.message ?? 'Unable to sign in. Please try again.'),
        );
      },
    });
  }

  private redirect(): void {
    this.loading.set(false);
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    const destination = this.auth.requiresPasswordChange()
      ? '/auth/change-password'
      : (returnUrl ?? this.auth.postAuthRoute());
    void this.router.navigateByUrl(destination);
  }
}
