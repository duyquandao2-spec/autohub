# Auth System — AutoHub (APEX MOTORS)

ASP.NET Core Identity Authentication & Account area for the Razor Pages application.

## Overview

- Framework: ASP.NET Core `net8.0`, Razor Pages.
- Identity: full ASP.NET Core Identity (ASP.NET Identity EF Core stores) with **ASP.NET Identity UI source added** (customized, not the NuGet UI package).
- Backing DB: SQL Server `AUTOHUB` (connection in `appsettings.json`).
- Public storefront UI and its own `_Layout.cshtml`/CSS are **unchanged**; all auth additions are additive.

## Auth-related files

- `Pages/Login.cshtml(.cs)` — **root** login page. Route `/Login` (no kebab-case), namespace `AutoHub.Pages`.
- `Pages/Register.cshtml(.cs)` — root register page, route `/Register`.
- `Pages/Logout.cshtml(.cs)` — GET + POST sign-out, redirects to `/Index`.
- `Pages/403.cshtml(.cs)` — access-denied page (route `/403`).
- `Pages/Error.cshtml(.cs)` — general error page (`[ResponseCache(NoStore=true)]`).
- `Pages/Auth/ForgotPassword.cshtml(.cs)` — route `/auth/forgotpassword`.
- `Pages/Auth/ResetPassword.cshtml(.cs)` — route `/auth/resetpassword`.
- `Pages/Account/*` — signed-in account area: `Index`, `Profile`, `Security`, `Favorites`, `Consultations`, `TestDrives`.
- `Pages/Shared/_AuthMenu.cshtml` — header auth partial (guest Đăng nhập/Đăng ký; signed-in dropdown with Đăng xuất POST form).
- `Pages/Shared/_LoginPartial.cshtml` — Identity-style login partial.
- `wwwroot/css/auth.css`, `wwwroot/js/auth.js` — auth styling/behaviour.
- `Services/Auth/AuthService.cs` (`IAuthService`) — wraps Identity `SignInManager`, `UserManager`; `SignInAsync`, `SignOutAsync`.

## Routing notes (important)

- Razor Pages in this app use the **default file-name routing on original casing** (no kebab-casing). E.g. `Pages/Auth/ForgotPassword.cshtml` → `/auth/forgotpassword`; `Pages/Admin/News/Index.cshtml` → `/Admin/News/Index`.
- Route matching is case-insensitive, so `/register`, `/login`, `/admin` all resolve.
- The `/Login` page uses a plain `@page` (no literal override) to guarantee `/Login`; a literal `@page "login"` override did **not** register a route in this app, while a parameterized override `@page "{id:int?}"` does.
- `RedirectToPage(...)`/`asp-page` must use the **canonical page name** (e.g. `/Account/Index`, `/Index`, `/Login`, `/Admin/News/Index`). Lowercase `/account`, `/index`, `/login` throw `InvalidOperationException: No page named ... matches the supplied values` → HTTP 500.

## Authentication flows

- **Login** (`POST /Login`): signs in by **username** (`tên đăng nhập`, `UserName`), respects lockout, sets `AutoHub.Auth` cookie, redirects to ReturnUrl or `/Account/Index`. Existing accounts created earlier use the email as their username, so they can still sign in with that same string.
- **Register** (`POST /Register`): creates a new user with a **username**, email, and **phone number** (all required) + role `Customer`, signs in automatically, redirects to `/Account/Index`.
- **Logout** (`POST /logout`): signs out and clears the auth cookie, redirects to `/Index`. The form in `_AuthMenu` includes a hidden `__RequestVerificationToken` (`@Html.AntiForgeryToken()`) — required, otherwise POST returns HTTP 400.
- **Anti-forgery**: cookie name `AutoHub.Antiforgery`. Every POST form must carry `__RequestVerificationToken`.

## Identity configuration (`Program.cs`)

- Cookie auth: `Cookie.Name = ".AspNet.SharedCookie"`-style → actually `AutoHub.Auth` (check `Program.cs`/`appsettings.json`), secure by default, sliding expiration.
- Lockout: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 minutes`.
- Password policy relaxed for seeded SuperAdmin: min length 6, must contain a digit and a lowercase letter (uppercase/special not required).
- SuperAdmin seeded on startup from `.env`:
  - `SuperAdmin__Email` (default `autocar@gmail.com`)
  - `SuperAdmin__Password` (default `quan2k6`)
  - `SuperAdmin__FullName`

## SuperAdmin default credentials

| Field | Value |
|-------|-------|
| UserName | `autocar` |
| Password | `quan2k6` |
| Email | `autocar@gmail.com` |

Login is by username (`autocar`). Seed config in `.env` (`SuperAdmin__Email`, `SuperAdmin__Password`, `SuperAdmin__FullName`); the username is derived as the `@`-prefix of `SuperAdmin__Email`. On startup the seeder also rewrites an existing seeded account's `UserName` to that prefix if it previously stored the full email. **Do not commit real credentials.**

## Key account routes (signed-in only)

| Route | Purpose |
|-------|---------|
| `/account` | Account home |
| `/account/profile` | Edit profile |
| `/account/security` | Change password / security |
| `/account/favorites` | Saved vehicles |
| `/account/consultations` | My consultation requests |
| `/account/testdrives` | My test-drive bookings |

## Verification (smoke-tested)

- Anon `/account` → 302 `/login?ReturnUrl=%2Faccount`; anon `/admin` → 302 `/login?ReturnUrl=%2Fadmin`.
- SuperAdmin login → all account + admin pages 200.
- Register new user → auto-login → `/account` 200; `/admin` blocked (RBAC → 403).
- POST `/logout` with valid token → signed out; `/account` then redirects to `/login`.
- GET `/logout` also signs out and redirects to `/Index`.
