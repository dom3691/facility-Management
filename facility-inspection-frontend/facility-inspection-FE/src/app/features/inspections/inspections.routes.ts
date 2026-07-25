import { Routes } from '@angular/router';

/** Inspections feature routes. Backs `/api/inspections` (+ the incidents queue). */
export const INSPECTIONS_ROUTES: Routes = [
  {
    path: '',
    title: 'Inspections · Facility Inspection',
    loadComponent: () =>
      import('./pages/inspection-queue/inspection-queue').then((m) => m.InspectionQueue),
  },
  {
    path: 'new/:incidentId',
    title: 'Record Inspection · Facility Inspection',
    loadComponent: () =>
      import('./pages/inspection-form/inspection-form').then((m) => m.InspectionForm),
  },
  {
    path: ':id',
    title: 'Inspection · Facility Inspection',
    loadComponent: () =>
      import('./pages/inspection-details/inspection-details').then((m) => m.InspectionDetails),
  },
];
