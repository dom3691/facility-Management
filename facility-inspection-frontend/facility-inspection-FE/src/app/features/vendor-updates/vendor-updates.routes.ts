import { Routes } from '@angular/router';

/** Vendor portal routes (Vendor/Admin — gated in app.routes). Backs `/api/vendor-updates`. */
export const VENDOR_UPDATES_ROUTES: Routes = [
  {
    path: '',
    title: 'My Work Orders · Facility Inspection',
    loadComponent: () =>
      import('./pages/my-work-orders/my-work-orders').then((m) => m.MyWorkOrders),
  },
  {
    path: ':workOrderId',
    title: 'Update Work Order · Facility Inspection',
    loadComponent: () =>
      import('./pages/vendor-update/vendor-update').then((m) => m.VendorUpdateComponent),
  },
];
