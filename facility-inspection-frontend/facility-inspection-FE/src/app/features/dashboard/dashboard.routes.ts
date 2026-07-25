import { Routes } from '@angular/router';

/** Dashboard feature routes. Backs `GET /api/dashboard/summary`. */
export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    title: 'Dashboard · Facility Inspection',
    loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
  },
];
