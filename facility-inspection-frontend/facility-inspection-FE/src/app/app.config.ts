import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
} from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideNativeDateAdapter } from '@angular/material/core';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

/**
 * Application-wide providers (composition root for the standalone app).
 *
 * - Zoneless change detection (Angular 20 stable). The foundation is signal-based;
 *   to fall back to Zone.js, swap `provideZonelessChangeDetection()` for
 *   `provideZoneChangeDetection({ eventCoalescing: true })`, add `zone.js` to the
 *   dependencies and list it under `polyfills` in angular.json.
 * - Router with lazy routes, component input binding (route params → @Input),
 *   scroll restoration and view transitions.
 * - HttpClient (fetch) with the functional auth + error interceptors.
 * - Async animations, required by Angular Material.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withInMemoryScrolling({ scrollPositionRestoration: 'top', anchorScrolling: 'enabled' }),
    ),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
    provideAnimationsAsync(),
    provideNativeDateAdapter(),
  ],
};
