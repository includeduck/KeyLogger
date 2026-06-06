# Progress

## 2026-06-06

### Completed
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

### Next
- Phase 1: implement registration, login, email verification simulation, password reset, and protected dashboard routes.
