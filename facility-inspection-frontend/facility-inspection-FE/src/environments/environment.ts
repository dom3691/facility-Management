/**
 * Production environment (default build).
 * `ng build` uses this file; `ng serve` / `--configuration development`
 * swaps it for `environment.development.ts` via angular.json fileReplacements.
 */
export const environment = {
  production: true,
  /** Base URL of the ASP.NET Core API, including the `/api` prefix. */
  apiUrl: 'https://localhost:44315/api',
  appName: 'Facility Inspection Automation System',
  /** localStorage key under which the JWT access token is persisted. */
  tokenStorageKey: 'fias.access_token',
};
