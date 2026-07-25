import { AppRole } from '../../core/models';

export interface NavItem {
  label: string;
  icon: string;
  route: string;
  /** When set, the item is shown only to users holding one of these roles. */
  roles?: AppRole[];
}

export interface NavGroup {
  heading: string;
  items: NavItem[];
}

/**
 * Sidebar navigation model. Items without `roles` are visible to every
 * authenticated user; role-scoped items are filtered in the sidebar and are
 * additionally protected by route guards.
 */
export const NAV_GROUPS: NavGroup[] = [
  {
    heading: 'Main Menu',
    items: [
      { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
      { label: 'Analytics', icon: 'insights', route: '/analytics' },
      { label: 'Notifications', icon: 'notifications', route: '/notifications' },
    ],
  },
  {
    heading: 'Management',
    items: [
      { label: 'Incidents', icon: 'report_problem', route: '/incidents' },
      { label: 'Inspections', icon: 'fact_check', route: '/inspections' },
      { label: 'Work Orders', icon: 'engineering', route: '/work-orders' },
      { label: 'Verifications', icon: 'verified', route: '/verifications', roles: [AppRole.Initiator, AppRole.Inspector, AppRole.Admin] },
    ],
  },
  {
    heading: 'Vendors & Data',
    items: [
      { label: 'Assignments', icon: 'assignment_ind', route: '/vendor-assignments', roles: [AppRole.Admin, AppRole.Inspector] },
      { label: 'My Work Orders', icon: 'update', route: '/vendor-updates', roles: [AppRole.Vendor, AppRole.Admin] },
      { label: 'Audit Logs', icon: 'history', route: '/audit-logs', roles: [AppRole.Admin] },
    ],
  },
  {
    heading: 'Administration',
    items: [
      { label: 'Users', icon: 'group', route: '/admin/users', roles: [AppRole.Admin] },
      { label: 'Facilities', icon: 'apartment', route: '/admin/facilities', roles: [AppRole.Admin] },
      { label: 'Locations', icon: 'place', route: '/admin/locations', roles: [AppRole.Admin] },
      { label: 'Vendors', icon: 'store', route: '/admin/vendors', roles: [AppRole.Admin] },
      { label: 'Reference Data', icon: 'category', route: '/admin/reference-data', roles: [AppRole.Admin] },
    ],
  },
];
