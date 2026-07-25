import { Routes } from '@angular/router';

/** Admin module routes (all gated to Admin in app.routes). */
export const ADMIN_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'vendors' },
  {
    path: 'users',
    title: 'Users · Admin',
    loadComponent: () => import('./pages/users/users').then((m) => m.AdminUsers),
  },
  {
    path: 'facilities',
    title: 'Facilities · Admin',
    loadComponent: () => import('./pages/facilities/facilities').then((m) => m.AdminFacilities),
  },
  {
    path: 'locations',
    title: 'Locations · Admin',
    loadComponent: () => import('./pages/locations/locations').then((m) => m.AdminLocations),
  },
  {
    path: 'vendors',
    title: 'Vendors · Admin',
    loadComponent: () => import('./pages/vendors/vendors').then((m) => m.AdminVendors),
  },
  {
    path: 'reference-data',
    title: 'Reference Data · Admin',
    loadComponent: () =>
      import('./pages/reference-data/reference-data').then((m) => m.AdminReferenceData),
  },
];
