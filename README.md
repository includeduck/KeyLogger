# KeyMapper

KeyMapper is a keyboard analytics platform with a Windows desktop collector, an ASP.NET Core backend API, a React dashboard, and SQL Server persistence.

The current repo implements Phase 0 from `doc/ImplementationOrder.md`: project setup, infrastructure, database schema setup, API health/JWT boilerplate, dashboard routes, and a desktop shell with a tray icon.

## Stack

- Desktop: C# WPF on .NET 8
- API: ASP.NET Core Web API on .NET 8
- Dashboard: React + Vite + TypeScript
- Database: SQL Server LocalDB by default
- Auth foundation: JWT bearer authentication

## Repository Layout

```text
db/
  DBschema.sql
doc/
  ImplementationOrder.md
  SRS.md
  TechStack.md
  UseCases.md
src/
  KeyMapper.Api/
  KeyMapper.Dashboard/
  KeyMapper.Desktop/
KeyMapper.sln
Progress.md
```

## Prerequisites

- .NET SDK 8.0.410 or compatible .NET 8 SDK
- Node.js and npm
- SQL Server LocalDB or SQL Server
- `sqlcmd` for applying `db/DBschema.sql`

## Setup

Restore/build the .NET projects:

```powershell
dotnet restore KeyMapper.sln
dotnet build KeyMapper.sln
```

Install/build the dashboard:

```powershell
cd src\KeyMapper.Dashboard
npm install
npm run build
```

Create the SQL Server database on LocalDB:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i db\DBschema.sql
```

The API uses this default connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=KeyMapperDb;Trusted_Connection=True;TrustServerCertificate=True
```

## Run

Start the API:

```powershell
dotnet run --project src\KeyMapper.Api
```

Health check:

```text
http://localhost:5147/api/health
```

Start the dashboard:

```powershell
cd src\KeyMapper.Dashboard
npm run dev
```

Dashboard routes:

```text
http://localhost:5173/login
http://localhost:5173/register
http://localhost:5173/dashboard
```

Run the desktop shell:

```powershell
dotnet run --project src\KeyMapper.Desktop
```

## Phase 0 Status

- API returns `/api/health` and has JWT bearer validation configured.
- Dashboard has login, register, and dashboard placeholder routes.
- Dashboard dev server proxies `/api` to the API on `http://localhost:5147`.
- Desktop app opens a WPF shell and minimizes to the notification area.
- `db/DBschema.sql` creates `KeyMapperDb` and the initial schema.
