import { Routes } from '@angular/router';

/** Incidents feature routes. Backs `GET/POST /api/incidents` (+ `/my`, `/{id}`). */
export const INCIDENTS_ROUTES: Routes = [
  {
    path: '',
    title: 'Incidents · Facility Inspection',
    loadComponent: () => import('./pages/incident-list/incident-list').then((m) => m.IncidentList),
  },
  {
    path: 'new',
    title: 'Report Incident · Facility Inspection',
    loadComponent: () =>
      import('./pages/create-incident/create-incident').then((m) => m.CreateIncident),
  },
  {
    path: ':id',
    title: 'Incident · Facility Inspection',
    loadComponent: () =>
      import('./pages/incident-details/incident-details').then((m) => m.IncidentDetails),
  },
];
