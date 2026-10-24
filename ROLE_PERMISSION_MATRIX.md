# Role & Permission Matrix — AutoHub (APEX MOTORS)

Policies are defined in `Program.cs` (`options.AddPolicy`) and reference role sets in `Models/RoleNames.cs`.

## Roles

| Role | Notes |
|------|-------|
| `Customer` | Default role for self-registered users. Storefront + personal account only. |
| `Sales` | Sales staff — manage appointments/consultations. |
| `ContentEditor` | Editorial staff — manage news/content. |
| `Manager` | Operational manager — appointments, content, vehicles/audit view. |
| `Administrator` | Full dashboard minus super-admin-only settings/role minting. |
| `SuperAdmin` | Highest privilege; system configuration + role management. |

## Privilege sets (`RoleNames`)

| Set | Roles |
|-----|-------|
| `VehicleWriters` | Administrator, SuperAdmin |
| `AppointmentManagers` | Sales, Manager, Administrator, SuperAdmin |
| `UserManagers` | Administrator, SuperAdmin |
| `ContentManagers` | ContentEditor, Manager, Administrator, SuperAdmin |
| `AuditViewers` | Manager, Administrator, SuperAdmin |
| `RoleManagers` | SuperAdmin |
| `SystemConfigurators` | SuperAdmin |

## Policies → required roles

| Policy (`AutoHub.Models.Policies`) | Required roles | Applies to |
|-------------------------------------|----------------|------------|
| `AdminArea` | Sales, ContentEditor, Manager, Administrator, SuperAdmin | `/Admin` dashboard & admin layout |
| `VehicleWrite` | Administrator, SuperAdmin | `/Admin/Vehicles` |
| `VehicleDelete` | Administrator, SuperAdmin | Vehicle delete operations |
| `AppointmentManagement` | Sales, Manager, Administrator, SuperAdmin | `/Admin/Consultations`, `/Admin/TestDrives` |
| `UserManagement` | Administrator, SuperAdmin | `/Admin/Users`, `/Admin/Users/Detail` |
| `ContentManagement` | ContentEditor, Manager, Administrator, SuperAdmin | `/Admin/News` (list + edit) |
| `AuditView` | Manager, Administrator, SuperAdmin | `/Admin/Audit` |
| `SecurityAdministration` | SuperAdmin | Role management |
| `SystemConfiguration` | SuperAdmin | `/Admin/Settings` |

## Matrix (role × permission)

Legend: ✔ = can access.

| Feature | Customer | Sales | ContentEditor | Manager | Administrator | SuperAdmin |
|---------|:--------:|:-----:|:-------------:|:-------:|:------------:|:----------:|
| Storefront / public pages | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Personal account (`/account*`) | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Admin dashboard (`/Admin`) | ✘ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Appointment mgmt (consultations, test drives) | ✘ | ✔ | ✘ | ✔ | ✔ | ✔ |
| Content mgmt (news) | ✘ | ✘ | ✔ | ✔ | ✔ | ✔ |
| Vehicle catalog mgmt | ✘ | ✘ | ✘ | ✘ | ✔ | ✔ |
| User management | ✘ | ✘ | ✘ | ✘ | ✔ | ✔ |
| Audit log | ✘ | ✘ | ✘ | ✔ | ✔ | ✔ |
| System configuration/settings | ✘ | ✘ | ✘ | ✘ | ✘ | ✔ |
| Role management (`SecurityAdministration`) | ✘ | ✘ | ✘ | ✘ | ✘ | ✔ |

`Administrator`/`SuperAdmin` cannot be re-assigned from the UI — assignable roles from `/Admin/Users` are only `Customer`, `Sales`, `ContentEditor`, `Manager` (see `Pages/Admin/Users/Index.cshtml.cs` lines ~37, and `Services/Auth/UserService.cs` line ~136).

## Enforcement

- Pages use `[Authorize(Policy = ...)]` on the page-model class (e.g. `[Authorize(Policy = Policies.UserManagement)]` on `Pages/Admin/Users/Index.cshtml.cs`).
- Navigation in `_AdminSidebar` hides menu items the current user cannot access (role checks inline).
- `_AuthMenu` shows the admin link only to staff roles.
- Anonymous access to protected pages → redirect to `/login?...`; authenticated-but-unauthorized → `/403?ReturnUrl=...`.