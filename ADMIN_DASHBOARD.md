# Admin Dashboard — AutoHub (APEX MOTORS)

Role-based administrative area for the Razor Pages application.

## Access

- All admin pages live under `Pages/Admin/*` and require the **`AdminArea`** authorization policy (route prefix `/Admin`, case-insensitive).
- Unauthorized/anonymous users are redirected to `/403?ReturnUrl=...`.
- Only roles with the `AdminArea` policy may access: `Manager`, `Administrator`, `SuperAdmin` (see `ROLE_PERMISSION_MATRIX.md`).

## Areas / pages

| Route | Page | Purpose |
|-------|------|---------|
| `/Admin` | `Pages/Admin/Index.cshtml` | Dashboard (KPIs, latest activity) |
| `/Admin/Users` | `Pages/Admin/Users/Index.cshtml` | User management |
| `/Admin/Users/Detail` | `Pages/Admin/Users/Detail.cshtml` | User detail (query string `?id=`) |
| `/Admin/Consultations` | `Pages/Admin/Consultations/Index.cshtml` | Consultation management |
| `/Admin/Consultations/Detail` | `Pages/Admin/Consultations/Detail.cshtml` | Consultation detail |
| `/Admin/TestDrives` | `Pages/Admin/TestDrives/Index.cshtml` | Test-drive bookings |
| `/Admin/TestDrives/Detail` | `Pages/Admin/TestDrives/Detail.cshtml` | Test-drive detail |
| `/Admin/News` | `Pages/Admin/News/Index.cshtml` | News list |
| `/Admin/News/Edit` | `Pages/Admin/News/Edit.cshtml` | Create/edit news (`@page "{id:int?}"` override) |
| `/Admin/Vehicles` | `Pages/Admin/Vehicles/Index.cshtml` | Vehicle catalog |
| `/Admin/Audit` | `Pages/Admin/Audit/Index.cshtml` | Audit log |
| `/Admin/Settings` | `Pages/Admin/Settings.cshtml` | System configuration |
| `_AdminPager` | `Pages/Shared/_AdminPager.cshtml` | Shared pagination partial |

## Dashboard

- `Pages/Admin/Index.cshtml.cs` → `Services/Admin/AdminDashboardService.cs` (`IAdminDashboardService`).
- Exposes KPI counts (users, consultations, test drives, news, vehicles, unread/featured variants) and recent activity.

## Policies (authorization requirements)

Defined in `Models/Policies.cs` (namespace `AutoHub.Models.Policies`):

- `AdminArea` — access the admin dashboard area.
- `UserManagement` — manage users / assign roles.
- `AppointmentManagement` — manage consultations & test drives.
- `ContentManagement` — manage news/vehicles/content.
- `AuditView` — view the audit log.
- `SystemConfiguration` — change system settings.

Each page/`@page` model declares the required `[Authorize(Policy = ...)]`.

## Detail pages

- Detail pages return `NotFound()` (HTTP 404) when the requested entity does not exist (e.g. empty `Consultations` table with `id=1` → 404); this is correct behaviour.
- Test drive `TD-000001` exists (`id=1`) → 200.

## Pagination

- `Pages/Admin/Vehicles/Index.cshtml(.cs)` and `Pages/Admin/Audit/Index.cshtml(.cs)` expose `public int TotalPages` (set in `OnGet`); views use `Model.TotalPages` and set `ViewData["TotalPages"]`.

## Staff name resolution

- After removing the `AssignedStaff` navigation (kept scalar `AssignedStaffId`), staff names are resolved at read time via a lookup on the ASP.NET Identity `Users` table:
  - `Services/TestDriveService.cs` (lines ~64-73)
  - `Services/ConsultationService.cs` (lines ~65-72)
- All destructive operations soft-delete (records flagged, not physically removed).

## Runtime verification

- SuperAdmin saw all admin pages return HTTP 200.
- `/Admin/Users/Detail?id=<real-su-id>` → 200; `/Admin/TestDrives/Detail?id=1` → 200.
- Redirects from admin-aware anonymous flow → 302 to `/login?...` / `/403?...`.
