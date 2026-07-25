# Database Setup

The backend uses **SQL Server** via **EF Core 8**. Two databases are used:

| Database | Purpose | Connection string |
| --- | --- | --- |
| `FacilityInspection` | Application data (all domain tables + Identity) | `ConnectionStrings:DefaultConnection` |
| `FacilityInspectionHangfire` | Hangfire background-job storage | `ConnectionStrings:HangfireConnection` |

## 1. Connection strings

Configured in `src/FacilityInspection.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FacilityInspection;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
  "HangfireConnection": "Server=localhost;Database=FacilityInspectionHangfire;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Adjust `Server=` for your environment, for example:

- **LocalDB:** `Server=(localdb)\\MSSQLLocalDB;...`
- **SQL auth:** `Server=localhost;Database=FacilityInspection;User Id=sa;Password=Your_Strong_Pass!;TrustServerCertificate=True`
- **Docker:** `Server=localhost,1433;...` (after `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_Strong_Pass!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`)

> Do not commit real credentials. For local development prefer `dotnet user-secrets`; in
> hosting environments use environment variables (`ConnectionStrings__DefaultConnection`).

## 2. Install the EF Core CLI (once)

```bash
dotnet tool install --global dotnet-ef
# or update: dotnet tool update --global dotnet-ef
```

## 3. Create / upgrade the application database

Migrations already exist in `src/FacilityInspection.Persistence/Migrations`, so you normally only need
to **apply** them. The Persistence project owns the migrations; the API project is the startup project.

```bash
dotnet ef database update \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API
```

This creates the `FacilityInspection` database (if missing) and all tables. The
`FacilityInspectionHangfire` database is created automatically by Hangfire the first time the API runs.

### Existing migrations (schema history)

```
20260721151425_configured_Auth_JWT_Identity_And_Roles   # Identity tables + roles
20260721174311_fileUploadsEntity                         # file/attachment metadata
20260721213449_addedVendor_and_workOrder                 # vendors + work orders
20260721215057_Updated_workOrder
20260721222710_addded_vendorUpdates                      # vendor progress updates
20260721224156_modified_workOrder
20260721230034_notificationServiceImplement_and_BackgroundJobs
20260721231838_added_AuditLogs                           # audit log table
```

These 8 migrations cover the **complete MVP schema**. The later Dashboard, security, and seed-user
work added **no schema changes** (read-only queries, authorization logic, and row inserts only), so no
further migration is required to run the current MVP.

## 4. Creating new migrations (developer workflow)

Migrations are **author-controlled** and never applied automatically at startup. When you change the
model, create a migration yourself and review it before applying:

```bash
dotnet ef migrations add <DescriptiveName> \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API \
  --output-dir Migrations

dotnet ef database update \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API
```

To check whether the model has drifted from the last migration:

```bash
dotnet ef migrations has-pending-model-changes \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API
```

## 5. Seed data

Seeding runs at startup via `DatabaseSeederHostedService` (idempotent — safe to run every launch). It
**does not** apply migrations; the tables must already exist (step 3).

| Seeded | Environment | Contents |
| --- | --- | --- |
| Roles | All | `Admin`, `Initiator`, `Inspector`, `Vendor` |
| Reference data | All | Facilities (`Head Office`, `Plant`, `Warehouse`); Locations under Head Office (`Reception`, `Restroom`, `Electrical Room`, `HVAC Area`, `Common Area`) |
| Sample vendor | **Development only** | `Sample Vendor Co` (ExternalVendor, active) |
| Sample users | **Development only** | One user per role (see below), linked vendor for the Vendor user |

### Development sample users

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@fis.local` | `Admin@123` |
| Initiator | `initiator@fis.local` | `Initiator@123` |
| Inspector | `inspector@fis.local` | `Inspector@123` |
| Vendor | `vendor@fis.local` (linked to `Sample Vendor Co`) | `Vendor@123` |

These credentials are seeded **only** when `ASPNETCORE_ENVIRONMENT=Development`. They are never created
in Production. Create real privileged users through the Admin-only `POST /api/auth/users` endpoint.

## 6. Verify

After `dotnet run --project src/FacilityInspection.API`:

- `GET /health/ready` should report **Healthy** (SQL Server + DbContext reachable).
- Log in via `POST /api/auth/login` with a seeded user.
- The Hangfire dashboard at `/hangfire` should list the `send-pending-notifications` recurring job.

## 7. Reset (local only)

```bash
dotnet ef database drop -f \
  --project src/FacilityInspection.Persistence \
  --startup-project src/FacilityInspection.API
# then re-run step 3
```
