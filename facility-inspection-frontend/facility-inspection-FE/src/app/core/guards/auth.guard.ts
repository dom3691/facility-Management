import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Allows the route only for an authenticated user. Otherwise redirects to the
 * login screen, preserving the attempted URL as `returnUrl`.
 * When a password change is required, redirects to the change-password screen.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    auth.logout(false);
    return router.createUrlTree(['/auth/login'], {
      queryParams: { returnUrl: state.url },
    });
  }

  if (auth.requiresPasswordChange() && !state.url.startsWith('/auth/change-password')) {
    return router.createUrlTree(['/auth/change-password']);
  }

  return true;
};
