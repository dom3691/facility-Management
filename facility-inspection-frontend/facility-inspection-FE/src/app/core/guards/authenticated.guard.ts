import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Requires a valid session but does not enforce password-change routing. */
export const authenticatedGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  auth.logout(false);
  return router.createUrlTree(['/auth/login']);
};
