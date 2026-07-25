# Backend Architecture

ASP.NET Core 8 · Clean Architecture · CQRS (MediatR) · EF Core 8 (SQL Server).

## 1. Layers & dependency rules

Dependencies point **inward only**. An outer layer may depend on an inner layer; never the reverse.

```
                    ┌───────────────────────────────┐
                    │           Domain              │  entities, enums, domain events
                    │      (no dependencies)        │
                    └───────────────▲───────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │         Application           │  CQRS features, DTOs, validators,
                    │  interfaces (ports), behaviours│  Result/PaginatedResult, exceptions
                    └──▲───────────────────────────▲─┘
                       │                           │
     ┌─────────────────┴───────┐        ┌──────────┴──────────────┐
     │      Persistence        │        │     Infrastructure      │
     │ EF Core DbContext,      │        │ JWT, current user, clock,│
     │ Identity, repos,        │        │ email, file storage,     │
     │ migrations, seed        │        │ Hangfire jobs            │
     └─────────────────▲───────┘        └──────────▲──────────────┘
                       │                           │
                    ┌──┴───────────────────────────┴──┐
                    │              API                 │  controllers, middleware,
                    │        (composition root)        │  Swagger, health, DI wiring
                    └──────────────────────────────────┘
```

**Verified project references:**

| Project | References |
| --- | --- |
| Domain | *(none)* |
| Application | Domain |
| Persistence | Application, Domain |
| Infrastructure | Application, Domain |
| API | Application, Infrastructure, Persistence |

Application depends on **no** infrastructure technology. It declares interfaces
(`IUnitOfWork`, `ICurrentUserService`, `IIdentityService`, `IFileStorageService`,
`INotificationService`, `IEmailService`, `IDateTimeProvider`, `IAuditService`) that Persistence and
Infrastructure implement. The API composes them at startup — layer by layer, dependencies flowing inward:

```csharp
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
```

## 2. Request pipeline

Two pipelines cooperate: the ASP.NET middleware pipeline and the MediatR behaviour pipeline.

**HTTP middleware (order matters):**

```
CorrelationId → RequestLogging → ExceptionHandling → Swagger(dev) → HttpsRedirect
             → CORS → Authentication → Authorization → Controllers / Health / Hangfire
```

**MediatR behaviours** wrap every command/query:

```
UnhandledException → Logging → Performance → Validation → Handler
```

A controller action sends a MediatR request; behaviours validate/log/time it; the handler runs the
use case against repositories and services; the result is mapped to a DTO and wrapped in the standard
`ApiResponse<T>` envelope by an MVC result filter.

## 3. Feature (vertical slice) structure

Each use case is a self-contained folder under `Application/Features/<Module>/<UseCase>`:

```
Features/Verifications/
├─ Common/                       # shared mappings + object-level authorization helpers
│  ├─ VerificationMappings.cs    # ToResponse() extension (entity → DTO)
│  └─ VerificationAuthorization.cs
├─ CreateVerification/
│  ├─ CreateVerificationCommand.cs
│  ├─ CreateVerificationCommandHandler.cs
│  └─ CreateVerificationCommandValidator.cs
├─ GetVerificationById/ …
└─ GetPendingVerifications/ …
```

Modules: `Authentication`, `Incidents`, `Inspections`, `Vendors`, `VendorAssignments`, `WorkOrders`,
`VendorUpdates`, `Verifications`, `Notifications`, `AuditLogs`, `Reference`, `Files`, `Dashboard`.

## 4. Domain model & workflow

**Core entities:** `Incident`, `Inspection`, `Vendor`, `VendorAssignment`, `WorkOrder`,
`VendorUpdate`, `Verification`, `Notification`, `AuditLog`, `Facility`, `Location`, plus attachment
entities. Identity user is `ApplicationUser` (Persistence layer, Guid key); domain entities reference
users by `Guid` (logical FKs), keeping Domain free of the Identity dependency.

**Status transitions:**

```
Incident:   PendingInspection → AwaitingVendorAssignment → VendorAssigned
              → WorkOrderCreated → AwaitingVerification → Closed
WorkOrder:  Assigned → InProgress → Completed → Closed
            (Verification = NotFixed returns Completed → InProgress for rework)
```

Enums are persisted as **strings** (`HasConversion<string>()`) for readable, stable data.
`WorkOrder → Verification` is 1-to-many to support the NotFixed → rework → re-verify loop.

## 5. Cross-cutting concerns

| Concern | Design |
| --- | --- |
| **Authentication** | ASP.NET Core Identity (Guid keys) issues JWT bearer tokens (`IJwtTokenGenerator`). `JwtSettings` bound from config. `/api/auth/login` returns an access token; `/api/auth/register` is anonymous and always creates an **Initiator**. |
| **Authorization** | Role-based (`[Authorize(Roles=…)]`) + named policies (`RequireAdmin/Initiator/Inspector/Vendor`) **plus** resource-based (object-level) checks in handlers so a vendor sees only their own work orders and an initiator only their own incidents. See [API_ENDPOINTS.md](API_ENDPOINTS.md). |
| **Validation** | FluentValidation validator per command; `ValidationBehaviour` short-circuits invalid requests into a `ValidationException` before the handler runs. |
| **Exception handling** | `ExceptionHandlingMiddleware` maps typed exceptions (`NotFound`, `Validation`, `ForbiddenAccess`, `BadRequest`, `UnauthorizedAccess`) to the correct status codes inside the `ApiResponse` envelope. |
| **API response consistency** | Every response is `ApiResponse<T> { success, message, data, errors[] }`, applied globally by `ApiResponseWrapperFilter`; controllers just return `Ok(dto)`. |
| **Audit logging** | `IAuditService` (`LogCreate/Update/Delete/WorkflowAction`) writes append-only `AuditLog` rows, enlisted in the caller's unit of work for atomic commits. No delete path. Query endpoints are Admin-only. |
| **Notifications** | `INotificationService.QueueNotificationAsync` persists `Notification` rows (Pending); a Hangfire recurring job (`send-pending-notifications`, `Cron.Minutely`) calls `SendPendingNotificationsAsync` to dispatch and mark them Sent. `IEmailService` is a logging stub for the MVP. |
| **File upload** | `IFileStorageService` → `LocalFileStorageService` stores files under `wwwroot/uploads/<module>/…` with unique names, preserving the original name in metadata. Type/size validated from `FileStorage` config. Binary is **not** stored in SQL. |
| **Persistence** | Repository + Unit of Work; typed repositories exposed on `IUnitOfWork`; generic `Repository<T>`. `IEntityTypeConfiguration<T>` per entity. Global query filter for soft delete (`ISoftDelete`); a SaveChanges interceptor stamps audit fields (`IAuditableEntity`). |
| **Migrations** | Author-controlled EF Core migrations in `Persistence/Migrations`. Never auto-applied at startup. |
| **Seed data** | `DatabaseSeederHostedService` seeds roles + reference data (facilities/locations) in all environments, and a sample vendor + one user per role in **Development only**. Idempotent. |
| **Logging** | Serilog replaces default providers; console + daily rolling file (`logs/`, 14 files retained), enriched with correlation id, machine name, thread id. Levels from config (Debug in Development). |
| **Health checks** | `/health` (full), `/health/ready` (SQL Server + `DbContext`, tag `ready`), `/health/live` (process). |
| **API docs** | Swagger/OpenAPI in Development, versioned via `Asp.Versioning`; JWT bearer configured in the UI. |

## 6. MVP readiness review

| Area | Status | Notes |
| --- | --- | --- |
| Clean Architecture dependency rules | ✅ | Verified; no inward layer references an outer one. |
| Naming consistency | ✅ | `*Command/Query`, `*Handler`, `*Validator`, `*Response`, `ToResponse()`, `AppRoles`/`AuthPolicies` constants (no magic strings). |
| Folder organisation | ✅ | Vertical slices under `Features/`; DTOs, Interfaces, Behaviours under `Common`. |
| DTO usage | ✅ | Entities never leave the API; static `ToResponse()` mappers; `PaginatedResult<T>` for lists. |
| Validation coverage | ✅ | Validator + `ValidationBehaviour` for every write command. |
| Exception handling | ✅ | Central middleware; typed exceptions → correct status codes. |
| Swagger | ✅ | Dev-only, versioned, JWT-aware. |
| Authentication | ✅ | Identity + JWT; inactive accounts rejected at login. |
| Authorization | ✅ | Role + policy + object-level ownership checks. |
| Audit logging | ✅ | Append-only, atomic, Admin-only reads. |
| Notification queue | ✅ | Persisted queue + Hangfire minutely dispatch. |
| File upload | ✅ | Disk storage, validated, metadata persisted, no SQL blobs. |
| EF Core mappings | ✅ | Per-entity configs, string enums, soft-delete filter, audit interceptor. |
| Database migrations | ✅ | 8 migrations covering the full schema; author-controlled. |
| Seed data | ✅ | Roles + reference always; sample users/vendor Development-only. |
| Appsettings structure | ✅ | Connection strings, JWT, CORS, FileStorage, Serilog sections; Development override. |
| Serilog configuration | ✅ | Console + rolling file, enrichers, per-source levels. |
| Hangfire configuration | ✅ | Separate DB, dashboard, recurring dispatch job. |
| Health checks | ✅ | live / ready / full. |
| API response consistency | ✅ | Global `ApiResponse` envelope + correlation id. |

**Out of MVP scope (intentionally not built):** SLA engine, escalation engine, advanced/trend
reporting, KPI dashboards, vendor scorecards, Power BI. These are Phase 2.
