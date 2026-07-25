import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AppRole } from '../models';

/**
 * Guard factory: allows the route only if the user holds at least one of the
 * given roles. Unauthenticated users go to login; authenticated-but-unauthorised
 * users are sent back to the dashboard.
 *
 * Usage: `canActivate: [roleGuard(AppRole.Admin, AppRole.Inspector)]`
 */
export function roleGuard(...roles: AppRole[]): CanActivateFn {
  return (_route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: state.url },
      });
    }

    return auth.hasAnyRole(...roles) ? true : router.createUrlTree(['/dashboard']);
  };
}
