import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Blocks the main app until the user changes a mandatory temporary password.
 * Allows only the change-password screen (and sign-out via login redirect).
 */
export const passwordChangeGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    auth.logout(false);
    return router.createUrlTree(['/auth/login'], {
      queryParams: { returnUrl: state.url },
    });
  }

  if (auth.requiresPasswordChange()) {
    return router.createUrlTree(['/auth/change-password']);
  }

  return true;
};
