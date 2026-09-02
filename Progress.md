# Progress

## 2026-06-23

### Completed (Phase 1)
- Added EF Core `KeyMapperDbContext` mapping to `Users`, `UserPreferences`, `RefreshTokens`, and `AuditLogs`.
- Implemented auth services: BCrypt password hashing, JWT + refresh tokens, console email simulation, audit logging.
- Added API endpoints: register, verify-email, login, refresh, logout, forgot/reset password, profile, change-password.
- Built dashboard auth layer (`AuthContext`, localStorage tokens, API client with 401 refresh, protected routes).
- Added pages: login, register, forgot/reset password, verify email, profile, and protected dashboard.
- Configured EF Core trigger awareness for SQL Server `Users` table triggers.
- Verified `dotnet build KeyMapper.sln`, `npm run build`, and end-to-end auth API flow.

### Next
- Phase 2: desktop client basic key collection (keyboard hook, in-memory counters, tray pause/resume).

---

## 2026-06-06

### Completed (Phase 0)
- Read the project documents in `doc/` and confirmed Phase 0 scope.
- Created the root .NET solution file and pinned the SDK with `global.json`.
- Scaffolded the ASP.NET Core Web API project in `src/KeyMapper.Api`.
- Scaffolded the WPF desktop project in `src/KeyMapper.Desktop`.
- Scaffolded the React + Vite dashboard project in `src/KeyMapper.Dashboard`.
- Installed API/dashboard dependencies and added the API/desktop projects to the solution.
- Configured JWT authentication boilerplate, CORS, Swagger, and `/api/health`.
- Added dashboard routing for `/login`, `/register`, and `/dashboard`.
- Added a dashboard API health badge through the Vite dev proxy.
- Added a WPF Phase 0 window with notification-area tray behavior.
- Updated the SQL Server schema script to create/use `KeyMapperDb`.
- Applied the schema to LocalDB and verified 10 tables plus 15 project indexes.
- Verified `dotnet build KeyMapper.sln`, `npm run build`, and the API health endpoint.
- Verified the login, register, and dashboard routes in the in-app browser.
