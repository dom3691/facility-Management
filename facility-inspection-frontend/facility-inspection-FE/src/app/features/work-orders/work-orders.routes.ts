import { Routes } from '@angular/router';

/** Work Orders feature routes. Backs `/api/work-orders` (+ `my-vendor-work-orders`, `/{id}`). */
export const WORK_ORDERS_ROUTES: Routes = [
  {
    path: '',
    title: 'Work Orders · Facility Inspection',
    loadComponent: () =>
      import('./pages/work-order-list/work-order-list').then((m) => m.WorkOrderList),
  },
  {
    path: ':id',
    title: 'Work Order · Facility Inspection',
    loadComponent: () =>
      import('./pages/work-order-details/work-order-details').then((m) => m.WorkOrderDetails),
  },
];
