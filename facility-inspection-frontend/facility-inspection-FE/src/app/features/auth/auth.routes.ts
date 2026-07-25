import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../core/guards/authenticated.guard';

/** Public authentication routes (rendered inside AuthLayout). */
export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    title: 'Sign in · Facility Inspection',
    loadComponent: () => import('./pages/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    title: 'Create account · Facility Inspection',
    loadComponent: () => import('./pages/register/register').then((m) => m.Register),
  },
  {
    path: 'forgot-password',
    title: 'Forgot password · Facility Inspection',
    loadComponent: () =>
      import('./pages/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },
  {
    path: 'reset-password',
    title: 'Reset password · Facility Inspection',
    loadComponent: () =>
      import('./pages/reset-password/reset-password').then((m) => m.ResetPassword),
  },
  {
    path: 'change-password',
    title: 'Change password · Facility Inspection',
    canActivate: [authenticatedGuard],
    loadComponent: () =>
      import('./pages/change-password/change-password').then((m) => m.ChangePassword),
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
];
