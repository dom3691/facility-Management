# Facility Inspection — Frontend (Angular 20)
Frontend foundation for the Facility Inspection Automation System. **Architecture only** —
the shell, routing, auth, and cross-cutting wiring are in place; feature *pages* are added later.

Consumes the ASP.NET Core backend (JWT-secured). Design tokens (Outfit/Inter/JetBrains Mono,
navy `#0D1B4B` / rust `#C74B30`) carried over from the approved prototype.

## Tech

Angular 20 (standalone, zoneless) · Angular Material (M3) · SCSS · RxJS · JWT auth · lazy routing.

## Folder structure

```
src/
├─ environments/
│  ├─ environment.ts               # production (default build)
│  └─ environment.development.ts   # dev (ng serve) — apiUrl → https://localhost:7209/api
├─ styles.scss                     # Material M3 theme + brand design tokens (CSS vars)
├─ index.html                      # Google Fonts + Material Icons
├─ main.ts                         # bootstrapApplication(App, appConfig)
└─ app/
   ├─ app.ts / app.html / app.scss # root shell (router-outlet only)
   ├─ app.config.ts                # providers: router, http+interceptors, animations, zoneless
   ├─ app.routes.ts                # top-level routing + lazy loading + guards
   │
   ├─ core/                        # singletons, app-wide (imported once)
   │  ├─ models/                   # ApiResponse, Paginated, Auth, User, Role, enums
   │  ├─ services/                 # AuthService (signals), TokenService (JWT)
   │  ├─ guards/                   # authGuard, roleGuard(...)
   │  └─ interceptors/             # authInterceptor, errorInterceptor
   │
   ├─ shared/                      # reusable, stateless building blocks
   │  └─ material/material.ts      # MATERIAL_IMPORTS bundle for standalone components
   │
   ├─ layouts/                     # route-level shells
   │  ├─ main-layout/              # authenticated: sidebar + toolbar
   │  └─ auth-layout/              # public: centred login backdrop
   │
   └─ features/                    # one lazy-loaded folder per module (routes are stubs for now)
      ├─ auth/  dashboard/  analytics/  notifications/
      ├─ incidents/  inspections/  work-orders/  verifications/
      └─ vendors/  vendor-assignments/  vendor-updates/  reference-data/  audit-logs/
```

> `services/`, `guards/`, `interceptors/`, `models/` live **under `core/`** — that's exactly
> what the "core" layer is (app-wide singletons). `shared/` holds reusable presentational pieces;
> `features/` holds domain modules; `layouts/` holds the shells.

## Dependency flow

```
features ─┐
layouts  ─┼─► core (services / guards / interceptors / models)  ─►  Angular / Material
shared   ─┘
```

- **core** depends on nothing internal (only Angular + environment). It's the single source of
  identity (`AuthService`), storage (`TokenService`), route protection and HTTP wiring.
- **shared** is presentational and stateless; it never imports from `features`.
- **features** and **layouts** depend on `core` (+ `shared`); features never import each other.
- Rule of thumb: dependencies point **inward** toward `core`. No cycles.

## Routing strategy

- Two shells: `/auth/*` (AuthLayout) and `/` (MainLayout, protected by `authGuard` +
  `canActivateChild`).
- Every feature is **lazy-loaded** with `loadChildren` → `features/<x>/<x>.routes.ts`.
- Role-gated areas use the `roleGuard` factory: `vendors` (Admin/Inspector), `audit-logs` (Admin).
- Unknown paths redirect to `/` (which redirects to `/dashboard`).

## HTTP interceptors (functional)

- `authInterceptor` — attaches `Bearer <token>` to API calls only (never to font/CDN URLs).
- `errorInterceptor` — on 401 clears the session and routes to login; unwraps the backend
  `ApiResponse` error envelope into `{ status, message, fieldErrors }` for features to display.

## Angular Material

- M3 theme configured in `styles.scss` via `mat.theme(...)` (azure primary / blue tertiary,
  Inter + Outfit typography, density 0). Brand colours are exposed as CSS custom properties
  (`--brand-navy`, `--brand-rust`, …) for direct use.
- `provideAnimationsAsync()` is registered in `app.config.ts`.
- Components import Material via the `MATERIAL_IMPORTS` bundle from `shared/`.
- **Next step for exact brand match:** generate a custom M3 palette from `#C74B30` / `#0D1B4B`
  (Angular Material schematic `ng generate @angular/material:theme-color`) and swap the built-in
  palettes in `styles.scss`.

## Environment configuration

`apiUrl` points at the backend (`https://localhost:7209/api`, matching the Kestrel HTTPS profile).
The backend CORS policy already allows `http://localhost:4200` (Angular's dev origin). `ng serve`
swaps `environment.ts` → `environment.development.ts` via `angular.json` file replacements.

## Run

```bash
npm install
npm start           # ng serve → http://localhost:4200
npm run build       # production build → dist/
```

Make sure the backend is running (`dotnet run --project ...API`) so the API + CORS are available.

## Change detection

The app runs **zoneless** (`provideZonelessChangeDetection()`), so state is signal-driven.
To use Zone.js instead: swap that provider for `provideZoneChangeDetection({ eventCoalescing: true })`,
add `zone.js` to dependencies, and list `"zone.js"` under `polyfills` in `angular.json`.

## What's intentionally NOT here (Phase 2)

SLA engine, escalation engine, advanced/trend reporting, Power BI — excluded on the backend, so
no frontend for them. The Analytics feature is a placeholder shell for now.
