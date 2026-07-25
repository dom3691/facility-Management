/**
 * Development environment. Active during `ng serve`.
 * The API URL matches the ASP.NET Core Kestrel HTTPS profile (launchSettings.json),
 * and the backend CORS policy already allows the Angular dev origin (http://localhost:4200).
 */
export const environment = {
  production: false,
  apiUrl: 'https://localhost:44315/api',
  appName: 'Facility Inspection Automation System',
  tokenStorageKey: 'fias.access_token',
};
