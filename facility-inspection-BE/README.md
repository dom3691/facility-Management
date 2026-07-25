# Facility Inspection Automation System — Backend

Enterprise backend for the Facility Inspection Automation System, built with
**ASP.NET Core 8** on a **Clean Architecture** foundation.

**MVP workflow:**
`Incident → Inspection → Vendor Assignment → Work Order → Progress → Completion → Verification → Closure`,
with **Notifications**, **Audit logging**, **File uploads**, **Reference data**, and a **Dashboard** as cross-cutting capabilities.

> Status: **MVP backend complete.** All workflow modules, authentication/authorization,
> auditing, notifications, file storage, health checks and a role-scoped dashboard are implemented,
> built (0 warnings) and covered by unit tests.

## Documentation map

| Document | What's inside |
| --- | --- |
| **README.md** (this file) | Overview, stack, quick start |
| [BACKEND_ARCHITECTURE.md](BACKEND_ARCHITECTURE.md) | Layers, dependency rules, cross-cutting design, MVP readiness review |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Connection strings, creating the DB, applying migrations, seed data |
| [API_ENDPOINTS.md](API_ENDPOINTS.md) | Every endpoint, method, and allowed roles |
| [MVP_TEST_FLOW.md](MVP_TEST_FLOW.md) | Step-by-step end-to-end workflow test with payloads and expected results |

## Technology stack

ASP.NET Core 8 Web API · Entity Framework Core 8 (SQL Server) · MediatR (CQRS) · FluentValidation ·
ASP.NET Core Identity + JWT · Serilog · Hangfire · Swagger/OpenAPI · API Versioning · Health Checks.

## Solution layout

```
facility-inspection-BE/
├─ Directory.Build.props           # Shared MSBuild props (net8.0, nullable, warnings-as-errors)
├─ Directory.Packages.props        # Central Package Management — every version pinned here
├─ global.json                     # Pins the .NET SDK (8.0.422)
├─ FacilityInspection.sln
├─ src/
│  ├─ FacilityInspection.Domain          # Entities, enums, domain events (no dependencies)
│  ├─ FacilityInspection.Application     # CQRS features, DTOs, validators, interfaces, behaviours
│  ├─ FacilityInspection.Persistence     # EF Core DbContext, Identity, migrations, repositories, seed
│  ├─ FacilityInspection.Infrastructure  # JWT, current user, clock, email, file storage, Hangfire jobs
│  └─ FacilityInspection.API             # Composition root: controllers, Swagger, health, middleware
└─ tests/
   └─ FacilityInspection.Tests           # xUnit + FluentAssertions + Moq
```

## Dependency direction

```
API ──► Application ──► Domain
 ├──► Infrastructure ──► Application ──► Domain
 └──► Persistence    ──► Application ──► Domain
```

Dependencies point **inward**. Domain depends on nothing. Application defines abstractions
(interfaces); Infrastructure and Persistence implement them. The API is the only project that
references the outer implementation layers, and only to compose the DI container at startup.

## Prerequisites

- **.NET 8 SDK** (pinned via `global.json` to 8.0.422)
- **SQL Server** (LocalDB, Docker, or a full instance) reachable via the `DefaultConnection`
  string in `src/FacilityInspection.API/appsettings.json`
- `dotnet-ef` tool (for migrations): `dotnet tool install --global dotnet-ef`

## Quick start

```bash
# 1. Restore & build
dotnet restore
dotnet build

# 2. Create/upgrade the database (migrations already exist in the repo)
dotnet ef database update \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API

# 3. Run (Development seeds roles, reference data, a sample vendor, and one user per role)
dotnet run --project src/FacilityInspection.API
```

Then open **https://localhost:7209/swagger** (or http://localhost:5095) and sign in with a seeded
Development user (see below). Full walkthrough: [MVP_TEST_FLOW.md](MVP_TEST_FLOW.md).

### Runtime endpoints (Development)

| Endpoint | Purpose |
| --- | --- |
| `/swagger` | API documentation / test UI |
| `/health` | Full health report (JSON) |
| `/health/ready` | Readiness (SQL Server + DbContext) |
| `/health/live` | Liveness (process up) |
| `/hangfire` | Background job dashboard (local requests only) |

### Seeded Development users

Seeded automatically on first run in the **Development** environment only (never in Production).

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@fis.local` | `Admin@123` |
| Initiator | `initiator@fis.local` | `Initiator@123` |
| Inspector | `inspector@fis.local` | `Inspector@123` |
| Vendor | `vendor@fis.local` | `Vendor@123` |

## Tests

```bash
dotnet test
```

## Configuration you must change before deploying

- `JwtSettings:SecretKey` — replace with a strong secret (≥ 32 chars), supplied out-of-band
  (user-secrets / environment variable), never committed.
- `ConnectionStrings:DefaultConnection` and `HangfireConnection` — point at your server.
- `Cors:AllowedOrigins` — set to your real frontend origin(s).

## Where does new code go?

| You are adding… | Put it in… |
| --- | --- |
| A business entity / enum | `Domain` |
| A use case (command/query + handler) | `Application/Features/<Feature>` |
| A DTO / response shape | `Application/DTOs` |
| Input validation | `Application` (FluentValidation validator) |
| A cross-cutting request rule | `Application/Common/Behaviours` |
| An abstraction for external tech | `Application/Common/Interfaces` |
| EF Core config, migration, repository, seed | `Persistence` |
| Email, JWT, clock, file storage, jobs | `Infrastructure` |
| A controller / endpoint | `API/Controllers` |
