import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppRole } from './core/models';

/**
 * Top-level routing strategy
 * --------------------------
 * Two shells:
 *   /auth/*  → AuthLayout   (public: login, etc.)
 *   /        → MainLayout   (protected by authGuard; hosts every feature)
 *
 * Every feature is **lazy-loaded** via `loadChildren` pointing at the feature's
 * own route file (`<feature>.routes.ts`). Role-restricted areas add `roleGuard`.
 * Feature route files are currently stubs — page components are added later.
 */
export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout').then((m) => m.AuthLayout),
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout').then((m) => m.MainLayout),
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
      },
      {
        path: 'analytics',
        loadChildren: () =>
          import('./features/analytics/analytics.routes').then((m) => m.ANALYTICS_ROUTES),
      },
      {
        path: 'notifications',
        loadChildren: () =>
          import('./features/notifications/notifications.routes').then(
            (m) => m.NOTIFICATIONS_ROUTES,
          ),
      },
      {
        path: 'incidents',
        loadChildren: () =>
          import('./features/incidents/incidents.routes').then((m) => m.INCIDENTS_ROUTES),
      },
      {
        path: 'inspections',
        loadChildren: () =>
          import('./features/inspections/inspections.routes').then(
            (m) => m.INSPECTIONS_ROUTES,
          ),
      },
      {
        path: 'work-orders',
        loadChildren: () =>
          import('./features/work-orders/work-orders.routes').then(
            (m) => m.WORK_ORDERS_ROUTES,
          ),
      },
      {
        path: 'verifications',
        canActivate: [roleGuard(AppRole.Initiator, AppRole.Inspector, AppRole.Admin)],
        loadChildren: () =>
          import('./features/verifications/verifications.routes').then(
            (m) => m.VERIFICATIONS_ROUTES,
          ),
      },
      {
        path: 'vendors',
        canActivate: [roleGuard(AppRole.Admin, AppRole.Inspector)],
        loadChildren: () =>
          import('./features/vendors/vendors.routes').then((m) => m.VENDORS_ROUTES),
      },
      {
        path: 'vendor-assignments',
        canActivate: [roleGuard(AppRole.Admin, AppRole.Inspector)],
        loadChildren: () =>
          import('./features/vendor-assignments/vendor-assignments.routes').then(
            (m) => m.VENDOR_ASSIGNMENTS_ROUTES,
          ),
      },
      {
        path: 'vendor-updates',
        canActivate: [roleGuard(AppRole.Vendor, AppRole.Admin)],
        loadChildren: () =>
          import('./features/vendor-updates/vendor-updates.routes').then(
            (m) => m.VENDOR_UPDATES_ROUTES,
          ),
      },
      {
        path: 'reference-data',
        loadChildren: () =>
          import('./features/reference-data/reference-data.routes').then(
            (m) => m.REFERENCE_DATA_ROUTES,
          ),
      },
      {
        path: 'audit-logs',
        canActivate: [roleGuard(AppRole.Admin)],
        loadChildren: () =>
          import('./features/audit-logs/audit-logs.routes').then((m) => m.AUDIT_LOGS_ROUTES),
      },
      {
        path: 'admin',
        canActivate: [roleGuard(AppRole.Admin)],
        canActivateChild: [roleGuard(AppRole.Admin)],
        loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
