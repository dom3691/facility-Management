# API Endpoints

Every MVP endpoint, its HTTP method, and the roles allowed to call it.

## Conventions

- **Base URL (dev):** `https://localhost:7209` (or `http://localhost:5095`).
- **Auth:** send `Authorization: Bearer <accessToken>` (from `POST /api/auth/login`) on all
  non-anonymous endpoints.
- **Response envelope:** every response is wrapped as
  `{ "success": bool, "message": string, "data": <payload>, "errors": [] }`.
- **Roles:** `Admin`, `Initiator`, `Inspector`, `Vendor`. **Auth** = any authenticated user.
- **+object** = the endpoint additionally enforces a resource-based (ownership) check in the handler,
  beyond the role gate.
- Multipart endpoints (file upload) are marked **`multipart`**; all others are JSON.

## Legend of scoping rules

- **Initiator own** — restricted to incidents/work the user reported.
- **Vendor own** — restricted to the vendor the user is linked to; one vendor can never see another's.
- **Admin/Inspector** — full operational visibility.

---

## Authentication — `/api/auth`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/register` | Anonymous | Self-service; **always creates an Initiator** (any supplied role is ignored). |
| POST | `/login` | Anonymous | Returns JWT access token + roles. Inactive accounts rejected. |
| GET | `/me` | Auth | Current user's profile + roles. |
| POST | `/users` | **Admin** | User management: creates a user with an explicit, validated role (the only path that can create privileged accounts). |

## Incidents — `/api/incidents`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Initiator, Admin | **multipart** — create incident with attachments. |
| GET | `/` | Admin, Inspector | Full list (operational queue), paged, optional status filter. |
| GET | `/my` | Auth | Incidents reported by the current user. |
| GET | `/{id}` | Auth **+object** | Admin/Inspector any; Initiator own; Vendor only if they hold a work order for it. |
| GET | `/pending-inspection` | Inspector, Admin | Incidents awaiting inspection. |

## Inspections — `/api/inspections`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Inspector, Admin | **multipart** — record an inspection (Good closes; Faulty routes to vendor assignment). |
| GET | `/{id}` | Auth | |
| GET | `/by-incident/{incidentId}` | Auth | |
| GET | `/my` | Auth | Inspections by the current inspector. |

## Vendors — `/api/vendors`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Admin | Create vendor (master data). |
| GET | `/` | Inspector, Admin | Paged list. |
| GET | `/{id}` | Inspector, Admin | |
| PUT | `/{id}` | Admin | Update vendor. |
| DELETE | `/{id}` | Admin | Soft delete. |

## Vendor Assignments — `/api/vendor-assignments`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Inspector, Admin | Assign an active vendor (category must match) to an inspected incident. |
| GET | `/{id}` | Auth | |
| GET | `/by-incident/{incidentId}` | Auth | |

## Work Orders — `/api/work-orders`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/generate` | Inspector, Admin | Generate a work order from a vendor assignment. |
| GET | `/` | Admin, Inspector | Paged list. |
| GET | `/{id}` | Auth **+object** | Admin/Inspector any; Vendor own; Initiator own incident. |
| GET | `/by-incident/{incidentId}` | Admin, Inspector | Cross-vendor enumeration — restricted. |
| GET | `/my-vendor-work-orders` | Auth (Vendor-scoped) | Work orders for the caller's vendor (empty if not vendor-linked). |
| PUT | `/{id}/status` | Vendor, Admin **+object** | Vendor may only update their own work orders. |

## Vendor Updates — `/api/vendor-updates`

Controller-gated to **Vendor, Admin**; all actions also enforce **+object** (vendor owns the work order).

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Vendor, Admin **+object** | **multipart** — progress update (first update moves work order to InProgress). |
| GET | `/by-work-order/{workOrderId}` | Vendor, Admin **+object** | |
| POST | `/{workOrderId}/mark-complete` | Vendor, Admin **+object** | **multipart** — marks work order Completed; incident → AwaitingVerification. |

## Verifications — `/api/verifications`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| POST | `/` | Initiator, Admin **+object** | Only the incident's initiator (or Admin) may verify. Fixed → closes; NotFixed → returns for rework. |
| GET | `/{id}` | Auth **+object** | Admin/Inspector; initiator own; vendor own. |
| GET | `/by-work-order/{workOrderId}` | Auth **+object** | |
| GET | `/pending` | Initiator, Inspector, Admin | Completed work orders awaiting verification (Initiator scoped to own). |

## Notifications — `/api/notifications`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| GET | `/` | Admin | All notifications, paged. |
| GET | `/my` | Auth | Current user's notifications. |
| PUT | `/{id}/mark-as-read` | Auth | Marks the caller's notification read. |

## Reference data — `/api/reference`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| GET | `/facilities` | Auth | |
| POST | `/facilities` | Admin | |
| GET | `/locations` | Auth | `?facilityId=` filter. |
| POST | `/locations` | Admin | |
| GET | `/inspection-classifications` | Auth | Enum options. |
| GET | `/vendor-categories` | Auth | Enum options. |
| GET | `/incident-statuses` | Auth | Enum options. |
| GET | `/work-order-statuses` | Auth | Enum options. |
| GET | `/verification-decisions` | Auth | Enum options. |

## Audit logs — `/api/audit-logs`

Controller-gated to **Admin** only. Append-only — no create/update/delete endpoints.

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| GET | `/` | Admin | Paged audit trail. |
| GET | `/by-entity/{entityName}/{entityId}` | Admin | Ordered trail for one entity. |

## Dashboard — `/api/dashboard`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| GET | `/summary` | Auth | Role-scoped counters (Initiator=own, Vendor=own, Inspector/Admin=all). |

## Files — `/api/files`

| Method | Path | Roles | Notes |
| --- | --- | --- | --- |
| GET | `/{fileId}` | Auth **+object** | File metadata; access checked against the owning record. |
| GET | `/download/{fileId}` | Auth **+object** | Streams the file. |

## Operational (non-API)

| Path | Access | Purpose |
| --- | --- | --- |
| `/swagger` | Dev only | API docs / test UI |
| `/health`, `/health/ready`, `/health/live` | Open | Health checks |
| `/hangfire` | Local requests | Background job dashboard |
