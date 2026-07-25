import { Routes } from '@angular/router';

/** Verifications feature routes (Initiator/Inspector/Admin — gated in app.routes). */
export const VERIFICATIONS_ROUTES: Routes = [
  {
    path: '',
    title: 'Verifications · Facility Inspection',
    loadComponent: () =>
      import('./pages/pending-verification/pending-verification').then((m) => m.PendingVerification),
  },
  {
    path: ':workOrderId',
    title: 'Verify Work Order · Facility Inspection',
    loadComponent: () =>
      import('./pages/verification-details/verification-details').then((m) => m.VerificationDetails),
  },
];
