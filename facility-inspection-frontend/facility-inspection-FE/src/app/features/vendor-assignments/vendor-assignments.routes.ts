import { Routes } from '@angular/router';

/** Vendor Assignments feature routes (Inspector/Admin — gated in app.routes). */
export const VENDOR_ASSIGNMENTS_ROUTES: Routes = [
  {
    path: '',
    title: 'Vendor Assignments · Facility Inspection',
    loadComponent: () =>
      import('./pages/assignment-queue/assignment-queue').then((m) => m.AssignmentQueue),
  },
  {
    path: ':incidentId',
    title: 'Assign Vendor · Facility Inspection',
    loadComponent: () =>
      import('./pages/vendor-assignment/vendor-assignment').then((m) => m.VendorAssignmentComponent),
  },
];
