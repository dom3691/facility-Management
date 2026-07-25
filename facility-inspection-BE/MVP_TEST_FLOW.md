# Facility Inspection — MVP Backend Test Flow

End-to-end manual / integration test script for the MVP workflow:

> **Incident → Inspection → Vendor Assignment → Work Order → Progress → Completion → Verification → Closure**, with **Audit** and **Notifications** as cross-cutting evidence.

Every step below lists the **endpoint**, **HTTP method**, **sample payload**, **expected response**, **database changes**, and **status transitions**.

---

## 0. Conventions

- **Base URL:** `https://localhost:5001` (adjust to your launch profile).
- **API version:** all business endpoints are under `/api/v1/...` if versioning is enabled, but the default route (`/api/...`) also resolves to v1. This doc uses `/api/...` for brevity.
- **Auth:** send `Authorization: Bearer <accessToken>` on every call after login.
- **Response envelope:** every response is wrapped by the global `ApiResponseWrapperFilter`:

  ```json
  {
    "success": true,
    "message": "Request successful",
    "data": { /* endpoint payload */ },
    "errors": []
  }
  ```

  Failures return `success: false`, a `message`, and a populated `errors` array (`ExceptionHandlingMiddleware`).
- **Content type:** JSON unless a step is marked **`multipart/form-data`** (those accept file uploads).
- **IDs** shown as `{...Id}` are GUIDs you copy from a previous response.

---

## 1. Seed users (Development only)

On startup in the **Development** environment, `DatabaseSeederHostedService` seeds roles, reference data (facilities/locations), a **sample vendor**, and one user per role. These credentials are **for local/dev use only** and are never seeded outside Development.

| Role      | Email                 | Password        | Notes                                             |
| --------- | --------------------- | --------------- | ------------------------------------------------- |
| Admin     | `admin@fis.local`     | `Admin@123`     | Full access                                       |
| Initiator | `initiator@fis.local` | `Initiator@123` | Raises incidents, verifies fixes                  |
| Inspector | `inspector@fis.local` | `Inspector@123` | Inspects, assigns vendors, generates work orders  |
| Vendor    | `vendor@fis.local`    | `Vendor@123`    | Pre-linked to vendor **“Sample Vendor Co”**       |

Seeded reference data you will reference below:

- **Facility:** `Head Office` (code `HO`)
- **Locations under HO:** `Reception`, `Restroom`, `Electrical Room`, `HVAC Area`, `Common Area`
- **Vendor:** `Sample Vendor Co` — category `ExternalVendor`, active, linked to `vendor@fis.local`

> Fetch the actual GUIDs at runtime via the Reference Data and Vendor endpoints (see step 2.a and 6.a).

---

## 2. Register / Login as Initiator

### 2.a (Optional) Register a new initiator

`POST /api/auth/register`

```json
{
  "firstName": "Ada",
  "lastName": "Reporter",
  "sapId": "SAP-1001",
  "email": "ada@fis.local",
  "phoneNumber": "+2348000000001",
  "password": "Ada@1234",
  "role": "Initiator"
}
```

**Expected:** `200 OK` — `data` contains `accessToken`, `expiresAtUtc`, and the user profile.
**DB:** new row in `AspNetUsers`; role mapping in `AspNetUserRoles`.

### 2.b Login (use the seeded initiator)

`POST /api/auth/login`

```json
{ "email": "initiator@fis.local", "password": "Initiator@123" }
```

**Expected `data`:**

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiresAtUtc": "2026-07-22T12:30:00Z",
  "userId": "{initiatorUserId}",
  "email": "initiator@fis.local",
  "roles": ["Initiator"]
}
```

➡️ Save `accessToken` as **INITIATOR_TOKEN**.

---

## 3. Create Incident (Initiator)

First get a facility + location id:

`GET /api/reference/facilities` → copy `Head Office` id (**FacilityId**).
`GET /api/reference/locations?facilityId={FacilityId}` → copy e.g. `Restroom` id (**LocationId**).

Then create the incident:

`POST /api/incidents`  — **`multipart/form-data`**, `Authorization: Bearer INITIATOR_TOKEN`

| Field          | Value                          |
| -------------- | ------------------------------ |
| `BusinessUnit` | `Facilities`                   |
| `SAPId`        | `SAP-1001`                     |
| `FacilityId`   | `{FacilityId}`                 |
| `LocationId`   | `{LocationId}`                 |
| `IncidentDate` | `2026-07-22`                   |
| `Description`  | `Leaking pipe in the restroom` |
| `attachments`  | *(optional file, e.g. jpg/pdf)* |

**Expected:** `201 Created` — `data`:

```json
{
  "id": "{incidentId}",
  "incidentNumber": "INC-2026-000001",
  "status": "PendingInspection",
  "businessUnit": "Facilities",
  "facilityId": "{FacilityId}",
  "locationId": "{LocationId}",
  "attachments": [ { "id": "...", "fileName": "pipe.jpg" } ]
}
```

**DB changes:**
- `Incidents` +1 (`Status = PendingInspection`, `ReportedByUserId = {initiatorUserId}`, number `INC-yyyy-000001`).
- `IncidentAttachments` +N (files saved to `wwwroot/uploads/incidents/...`, not in SQL).
- `AuditLogs` +1 (`Action = Created`, entity `Incident`).
- `Notifications` +N (queued to Inspectors/Admin).

**Status transition:** *Incident:* — → **PendingInspection**

➡️ Save **incidentId**.

---

## 4. Login as Inspector & view Pending Inspection

`POST /api/auth/login` → `{ "email": "inspector@fis.local", "password": "Inspector@123" }`
➡️ Save **INSPECTOR_TOKEN**.

`GET /api/incidents/pending-inspection?pageNumber=1&pageSize=20` — `Authorization: Bearer INSPECTOR_TOKEN`

**Expected:** `200 OK` — paginated `data` containing the incident from step 3 with `status: "PendingInspection"`.

```json
{
  "items": [ { "id": "{incidentId}", "incidentNumber": "INC-2026-000001", "status": "PendingInspection" } ],
  "pageNumber": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1
}
```

---

## 5. Create Inspection (Inspector)

`POST /api/inspections` — **`multipart/form-data`**, `Authorization: Bearer INSPECTOR_TOKEN`

| Field            | Value                                             |
| ---------------- | ------------------------------------------------- |
| `IncidentId`     | `{incidentId}`                                    |
| `Classification` | `Faulty`  *(a fault requiring a vendor)*          |
| `Comments`       | `Pipe joint cracked; vendor repair required.`     |
| `attachments`    | *(optional inspection photo)*                     |

**Expected:** `201 Created` — `data`:

```json
{
  "id": "{inspectionId}",
  "incidentId": "{incidentId}",
  "classification": "Faulty",
  "requiresVendor": true,
  "inspectedByUserId": "{inspectorUserId}"
}
```

**DB changes:**
- `Inspections` +1 (`InspectorUserId = {inspectorUserId}`).
- `InspectionAttachments` +N.
- `Incidents.Status` → `AwaitingVendorAssignment`.
- `AuditLogs` +1 (`Action = InspectionCompleted`).
- `Notifications` +N (vendor-assignment needed).

**Status transition:** *Incident:* PendingInspection → **AwaitingVendorAssignment**

➡️ Save **inspectionId**.

> A `Good` classification would instead close the incident (no vendor needed) — out of scope for the fix-path flow.

---

## 6. Assign Vendor (Inspector)

### 6.a Get the vendor id

`GET /api/vendors?pageNumber=1&pageSize=20` — `Authorization: Bearer INSPECTOR_TOKEN`
→ copy `Sample Vendor Co` id (**VendorId**). Its category is `ExternalVendor`.

### 6.b Create the assignment

`POST /api/vendor-assignments` — JSON, `Authorization: Bearer INSPECTOR_TOKEN`

```json
{
  "incidentId": "{incidentId}",
  "inspectionId": "{inspectionId}",
  "vendorId": "{VendorId}",
  "vendorCategory": "ExternalVendor",
  "notes": "Assign to external plumbing vendor."
}
```

**Expected:** `201 Created` — `data`:

```json
{
  "id": "{vendorAssignmentId}",
  "incidentId": "{incidentId}",
  "vendorId": "{VendorId}",
  "vendorCategory": "ExternalVendor"
}
```

**DB changes:**
- `VendorAssignments` +1.
- `Incidents.Status` → `VendorAssigned`.
- `AuditLogs` +1 (`Action = VendorAssigned`).
- `Notifications` +N (vendor notified).

**Status transition:** *Incident:* AwaitingVendorAssignment → **VendorAssigned**

➡️ Save **vendorAssignmentId**.

---

## 7. Generate Work Order (Inspector)

`POST /api/work-orders/generate` — JSON, `Authorization: Bearer INSPECTOR_TOKEN`

```json
{
  "vendorAssignmentId": "{vendorAssignmentId}",
  "description": "Repair cracked pipe joint in HO restroom."
}
```

**Expected:** `201 Created` — `data`:

```json
{
  "id": "{workOrderId}",
  "workOrderNumber": "WO-2026-000001",
  "incidentId": "{incidentId}",
  "vendorId": "{VendorId}",
  "status": "Assigned"
}
```

**DB changes:**
- `WorkOrders` +1 (`Status = Assigned`, number `WO-yyyy-000001`).
- `Incidents.Status` → `WorkOrderCreated`.
- `AuditLogs` +1 (`Action = WorkOrderGenerated`).
- `Notifications` +N (vendor notified of work order).

**Status transitions:** *Incident:* VendorAssigned → **WorkOrderCreated**; *Work Order:* — → **Assigned**

➡️ Save **workOrderId**.

---

## 8. Login as Vendor & view assigned Work Order

`POST /api/auth/login` → `{ "email": "vendor@fis.local", "password": "Vendor@123" }`
➡️ Save **VENDOR_TOKEN**. *(This user is pre-linked to `Sample Vendor Co`, the vendor assigned in step 6.)*

`GET /api/work-orders/my-vendor-work-orders?pageNumber=1&pageSize=20` — `Authorization: Bearer VENDOR_TOKEN`

**Expected:** `200 OK` — paginated list containing `WO-2026-000001` with `status: "Assigned"`.

---

## 9. Update Progress (Vendor)

`POST /api/vendor-updates` — **`multipart/form-data`**, `Authorization: Bearer VENDOR_TOKEN`

| Field                | Value                                    |
| -------------------- | ---------------------------------------- |
| `WorkOrderId`        | `{workOrderId}`                          |
| `ProgressComment`    | `Old joint removed; new fitting ordered.`|
| `ProgressPercentage` | `50`                                     |
| `attachments`        | *(optional progress photo)*             |

**Expected:** `201 Created` — `data` echoes the update with `isCompletionUpdate: false`.

**DB changes:**
- `VendorUpdates` +1.
- `VendorUpdateAttachments` +N.
- `WorkOrders.Status` → `InProgress` (first progress update moves it off `Assigned`).
- `AuditLogs` +1 (`Action = VendorProgressUpdate`).

**Status transition:** *Work Order:* Assigned → **InProgress**

---

## 10. Mark Work Order Complete (Vendor)

`POST /api/vendor-updates/{workOrderId}/mark-complete` — **`multipart/form-data`**, `Authorization: Bearer VENDOR_TOKEN`

| Field               | Value                                        |
| ------------------- | -------------------------------------------- |
| `CompletionComment` | `New joint installed and pressure-tested.`   |
| `attachments`       | *(optional completion evidence)*            |

**Expected:** `200 OK` — `data` with `isCompletionUpdate: true`, work order `status: "Completed"`.

**DB changes:**
- `VendorUpdates` +1 (`IsCompletionUpdate = true`, `CompletionComment` set).
- `WorkOrders.Status` → `Completed`.
- `Incidents.Status` → `AwaitingVerification`.
- `AuditLogs` +1 (`Action = VendorMarkedComplete`).
- `Notifications` +N (initiator + admin: ready for verification).

**Status transitions:** *Work Order:* InProgress → **Completed**; *Incident:* WorkOrderCreated → **AwaitingVerification**

---

## 11. Login as Initiator & view Pending Verification

`POST /api/auth/login` → seeded initiator → **INITIATOR_TOKEN**.

`GET /api/verifications/pending?pageNumber=1&pageSize=20` — `Authorization: Bearer INITIATOR_TOKEN`

**Expected:** `200 OK` — paginated list containing `WO-2026-000001` (`status: "Completed"`), scoped to the initiator’s own incidents.

---

## 12. Verify as Fixed (Initiator)

`POST /api/verifications` — JSON, `Authorization: Bearer INITIATOR_TOKEN`

```json
{
  "workOrderId": "{workOrderId}",
  "decision": "Fixed",
  "comments": "Confirmed leak resolved. Closing."
}
```

**Expected:** `201 Created` — `data`:

```json
{
  "id": "{verificationId}",
  "workOrderId": "{workOrderId}",
  "decision": "Fixed",
  "verifiedByUserId": "{initiatorUserId}"
}
```

**DB changes:**
- `Verifications` +1 (`Decision = Fixed`).
- `WorkOrders.Status` → `Closed`.
- `Incidents.Status` → `Closed`.
- `AuditLogs` +1 (`Action = VerificationFixed` / workflow closure).
- `Notifications` +N (initiator/inspector/vendor: closed).

**Status transitions:** *Work Order:* Completed → **Closed**; *Incident:* AwaitingVerification → **Closed**

> A `NotFixed` decision instead re-opens the work order (`InProgress`) and returns the incident to `WorkOrderCreated` for rework + re-verification (WorkOrder→Verification is 1:many).

---

## 13. Confirm Incident & Work Order are Closed

`GET /api/incidents/{incidentId}` — Initiator/Admin → `data.status == "Closed"`.
`GET /api/work-orders/{workOrderId}` — Inspector/Admin → `data.status == "Closed"`.

---

## 14. Confirm Audit Logs exist (Admin)

`POST /api/auth/login` → `admin@fis.local` → **ADMIN_TOKEN**.

`GET /api/audit-logs/by-entity/Incident/{incidentId}` — `Authorization: Bearer ADMIN_TOKEN`

**Expected:** `200 OK` — ordered trail for the incident, e.g.:

```
Created → InspectionCompleted → VendorAssigned → WorkOrderGenerated →
VendorProgressUpdate → VendorMarkedComplete → VerificationFixed
```

`GET /api/audit-logs?pageNumber=1&pageSize=50` returns the full (Admin-only) log. Audit logs are **append-only** — no delete endpoint exists.

---

## 15. Confirm Notifications exist

`GET /api/notifications/my` — as any recipient (e.g. INITIATOR_TOKEN) → the user’s notifications.
`GET /api/notifications?pageNumber=1&pageSize=50` — Admin → all notifications.

**Expected:** entries for incident-created, inspection-complete, vendor-assigned, work-order-generated, completion, and closure. A Hangfire recurring job dispatches queued notifications (`Status: Pending → Sent`). Mark one read:

`POST /api/notifications/{notificationId}/mark-as-read` → `IsRead = true`, `ReadAtUtc` set.

---

## Status transition reference

**Incident** (`IncidentStatus`):

```
PendingInspection → AwaitingVendorAssignment → VendorAssigned
  → WorkOrderCreated → AwaitingVerification → Closed
```

**Work Order** (`WorkOrderStatus`):

```
Assigned → InProgress → Completed → Closed
         (NotFixed verification: Completed → InProgress → …)
```

---

## MVP readiness checklist

Run the flow above once end-to-end, then confirm:

- [ ] **Auth** — register + login issue a JWT; `[Authorize]` blocks anonymous calls; role policies enforce access (e.g. Initiator cannot inspect).
- [ ] **Seed data** — Admin/Initiator/Inspector/Vendor users, Head Office facility + locations, and `Sample Vendor Co` exist in Development.
- [ ] **Incident** — created with `INC-yyyy-000001`, `Status = PendingInspection`, attachments on disk (not SQL).
- [ ] **Inspection** — Faulty inspection moves incident to `AwaitingVendorAssignment`.
- [ ] **Vendor assignment** — moves incident to `VendorAssigned`; category matches an active vendor.
- [ ] **Work order** — `/generate` produces `WO-yyyy-000001` (`Assigned`) and moves incident to `WorkOrderCreated`.
- [ ] **Vendor scoping** — vendor sees only their own work orders via `/my-vendor-work-orders`.
- [ ] **Progress** — first update moves work order to `InProgress`.
- [ ] **Completion** — `mark-complete` sets work order `Completed` and incident `AwaitingVerification`.
- [ ] **Verification (Fixed)** — closes both work order and incident; **NotFixed** returns them for rework.
- [ ] **Closure confirmed** — incident and work order both report `Closed`.
- [ ] **Audit** — a complete, append-only trail exists per entity (Admin-only, no delete).
- [ ] **Notifications** — queued at each step, visible via `/my` and dispatched by Hangfire.
- [ ] **Dashboard** — `GET /api/dashboard/summary` returns role-scoped counters consistent with the above.
- [ ] **API robustness** — all responses use the `ApiResponse` envelope; errors return structured `errors`; `GET /health` is green; correlation id present.
- [ ] **Build & tests** — `dotnet build` succeeds with 0 warnings; `dotnet test` passes.

> ✅ When every box is checked, the MVP backend workflow is functionally complete and ready for frontend integration / demo.
