/**
 * Application roles — must match the backend `AppRoles` constants exactly
 * (they arrive as string claims in the JWT).
 */
export const AppRole = {
  Admin: 'Admin',
  Initiator: 'Initiator',
  Inspector: 'Inspector',
  Vendor: 'Vendor',
} as const;

export type AppRole = (typeof AppRole)[keyof typeof AppRole];

export const ALL_ROLES: readonly AppRole[] = Object.values(AppRole);
