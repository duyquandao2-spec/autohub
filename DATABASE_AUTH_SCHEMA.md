# Database — Authentication & Business Schema (AutoHub)

Backing database: **SQL Server `AUTOHUB`** (server `LAPTOP-164VA9GO`).
Migration applied: `20260831030258_AddIdentityAndBusinessAuth` (`Migrations/`).

## Identity tables (ASP.NET Core Identity EF stores)

Standard ASP.NET Core Identity schema, created by the migration:

| Table | Purpose |
|-------|---------|
| `AspNetUsers` | Application users (email, hashed password, security stamp, lockout fields, etc.) |
| `AspNetRoles` | Roles |
| `AspNetUserRoles` | User↔Role join |
| `AspNetUserClaims` | Per-user claims |
| `AspNetRoleClaims` | Per-role claims |
| `AspNetUserLogins` | External login providers |
| `AspNetUserTokens` | Authentication/refresh tokens |

Indexes: `RoleNameIndex`, `EmailIndex`, `UserNameIndex` (standard Identity).

## Application/wp identity `ApplicationUser`

- Extends `IdentityUser`.
- Adds business properties (FullName, etc.) — see `Models`/`Data`.
- `AssignedStaffId` on `TestDriveBookings`/`ConsultationRequests` references `AspNetUsers.Id` by value (scalar FK to `Users`); the old `AssignedStaff` navigation was **removed** (still resolves via `_db.Users` lookup at read time).

## Business tables added by the auth migration

| Table | Purpose |
|-------|---------|
| `NewsCategories` | News categories |
| `NewsArticles` | News/articles |
| `ConsultationRequests` | Customer consultation requests (status, assigned staff) |
| `TestDriveBookings` | Test-drive bookings (see columns below) |
| `Favorites` | Saved-vehicle pairs (user ↔ vehicle) |
| `Showrooms` | Showroom locations |
| `SiteSettings` | Key/value system configuration |
| `AuditLogs` | Audit trail |
| `UserNotifications` | Per-user notifications |
| `StaffNotes` | Internal staff notes |
| `VehicleVariants` | Vehicle catalog (extended with featured/new/popular flags) |

## `TestDriveBookings` changes in this migration

- `Message` renamed → `Notes`.
- Added: `AssignedStaffId` (nvarchar(450), nullable → user id), `ContactPhone`, `ShowroomId` (int, nullable), `UpdatedAt` (datetime2), `UserId` (nvarchar(450), nullable → owning user).
- `UserId` is set at booking creation from `ICurrentUser`.

## Roles

`AspNetRoles` seeded with: `Customer`, `Sales`, `ContentEditor`, `Manager`, `Administrator`, `SuperAdmin`.

## SuperAdmin seed

Created at startup if missing, from `.env`:
- `SuperAdmin__Email` = `autocar@gmail.com`
- `SuperAdmin__Password` = `quan2k6`
- `SuperAdmin__FullName`

Password policy relaxed for the seed (min 6 + digit + lowercase; uppercase/special not required).

## EF Core tooling note

`dotnet-ef` is **not** on `PATH` in this environment. Invoke it explicitly:

```powershell
& "$env:USERPROFILE\.dotnet\tools\dotnet-ef.exe" database update
```

## Legacy data preserved

Migration is additive; existing data intact after apply: 32 vehicle variants, 15 brands, 1 test-drive booking (`TD-000001`, id=1).

## Backup

`...\APEXStudio\AUTOHUB_DATABASE\AUTOHUB_20260831_094852.bak`
