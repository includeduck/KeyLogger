# Implementation Order

## Keyboard Analytics System

**Version:** 1.0  
**Date:** 2026-06-06  
**Based on:** SRS v2.0, Use Cases v2.0, TechStack.md

---

## 1. Guiding Principles

- **Build incrementally** – start with a working core, then add features.
- **End-to-end slices first** – ensure each slice is demonstrable (e.g., desktop client talks to API, API talks to DB, dashboard shows data).
- **Resume-value focus** – prioritize features that showcase full-stack, desktop, and database skills.
- **Stretch goals last** – complex features (2FA, advanced admin, real-time) come after the core is stable.

---

## 2. Development Phases Overview

| Phase | Theme | Duration (estimate) |
|-------|-------|---------------------|
| 0 | Project Setup & Infrastructure | 1–2 days |
| 1 | Core User Management & Auth | 3–4 days |
| 2 | Desktop Client – Basic Collection | 5–7 days |
| 3 | Local Storage & Sync to Backend | 4–5 days |
| 4 | Dashboard – View Statistics | 5–7 days |
| 5 | Data Export & Reporting | 3–4 days |
| 6 | Data Deletion & Privacy Features | 2–3 days |
| 7 | Multi‑Device Support | 2–3 days |
| 8 | Admin Features (basic) | 2–3 days |
| 9 | Stretch Goals (real-time, 2FA, audit logs) | optional |

---

## 3. Detailed Implementation Steps

### Phase 0: Project Setup & Infrastructure

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Initialize Git repository, create solution structure (API, Desktop, Dashboard projects) | All | – |
| Set up SQL Server database, create initial tables: `Users`, `Devices`, `KeyCounts`, `SyncLogs` | Backend, DB | FR-6.3, FR-7 |
| Configure ASP.NET Core Web API project with JWT authentication boilerplate | Backend | FR-1.5, FR-2.5 |
| Create React + Vite project, set up routing (login, register, dashboard placeholder) | Dashboard | FR-8.1 |
| Set up desktop client project (C# WPF) with basic window and tray icon | Desktop | FR-2.1 |

**Deliverable:** Empty solution that builds, SQL Server running, API returns health check, dashboard shows login screen.

---

### Phase 1: Core User Management & Authentication

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Implement registration endpoint (username, email, password, ToS) | Backend, DB | FR-1.1, FR-1.2, FR-1.4, UC-1 |
| Implement email verification (basic: generate token, send via console/SMTP) | Backend | FR-1.3 |
| Implement login endpoint (username/email + password) → returns JWT access token | Backend | FR-1.5, FR-1.7, UC-2 |
| Implement password reset flow (forgot password, email reset link) | Backend | FR-1.10, UC-4 |
| Build registration and login pages in React, store JWT in localStorage/httpOnly cookie | Dashboard | UC-1, UC-2 |
| Protect dashboard routes with auth check | Dashboard | – |
| Add basic profile page (view email, change password) | Backend + Dashboard | FR-1.11 |

**Deliverable:** Users can register, verify email (simulated), log in, and view profile.

---

### Phase 2: Desktop Client – Basic Key Collection

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Implement low-level keyboard hook (WH_KEYBOARD_LL) in WPF | Desktop | FR-3.1, UC-6 |
| Capture key down events, extract scan code, virtual key, modifier state | Desktop | FR-3.2, FR-3.3, FR-3.13 |
| Apply simple debounce (ignore repeats within 50 ms) | Desktop | FR-3.2 |
| In-memory counter (dictionary per key) for current session | Desktop | FR-3.4 |
| Log each key press to console for debugging | Desktop | – |
| Add pause/resume feature to stop collection via tray menu | Desktop | FR-3.15 |
| Store authentication token locally (encrypted with DPAPI) | Desktop | FR-2.6 |

**Deliverable:** Client runs in system tray, counts key presses while active, shows total on hover.

---

### Phase 3: Local Storage & Sync to Backend

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Integrate SQLite into desktop client | Desktop | FR-4.1 |
| Create local table `key_events` (key_code, timestamp, device_id) or aggregated counters | Desktop | FR-4.1 |
| Periodically (every 5 min) flush in-memory counters to SQLite | Desktop | FR-4.3, FR-4.5 |
| Implement local encryption (AES-256 key derived from user’s password) | Desktop | FR-4.2 |
| Build API endpoint `POST /api/sync` – accepts JSON payload with per‑key increments | Backend | FR-6.1, FR-6.2, UC-7 |
| Implement sync logic in client: read unsynced data from SQLite, send to API, mark synced | Desktop + Backend | FR-5.1, FR-5.4, FR-5.5 |
| Add manual sync button in tray menu | Desktop | FR-5.2, UC-8 |
| Show sync status (last sync, pending events) in tooltip | Desktop | FR-5.3 |

**Deliverable:** Client collects keys offline, stores locally, syncs to backend on network, API stores data in SQL Server (`KeyCounts` table aggregated by hour/day).

---

### Phase 4: Dashboard – View Statistics

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Create API endpoint `GET /api/stats/total` (lifetime, today, this month) | Backend | FR-8.2, UC-9 |
| Create API endpoint `GET /api/stats/per-key` (list all keys with counts) | Backend | FR-8.3 |
| Create API endpoint `GET /api/stats/trend` (daily aggregates for line chart) | Backend | FR-8.8 |
| Build dashboard home page in React showing cards (total keys, today, month) | Dashboard | UC-9 |
| Build sortable table of key counts (key name, count, % of total) | Dashboard | FR-8.3 |
| Integrate Recharts – line chart for historical trend (last 30 days) | Dashboard | FR-8.8 |
| Add date range picker (presets: today, this week, this month) – frontend only initially | Dashboard | FR-9.1, UC-11 |
| Add dark/light theme toggle | Dashboard | FR-8.12 |

**Deliverable:** Logged-in user sees total key counts, a list of keys, and a trend line chart.

---

### Phase 5: Keyboard Heatmap & Advanced Visualization

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Create API endpoint `GET /api/stats/heatmap` (per-key counts for selected period) | Backend | FR-8.6 |
| Build SVG keyboard layout (simple US ANSI) in React component | Dashboard | FR-8.6 |
| Map key codes to SVG rectangles, color based on usage (light yellow → dark red) | Dashboard | FR-8.6 |
| Add click interaction: show exact count modal for clicked key | Dashboard | UC-10 |
| Integrate date range filter with API (re-fetch heatmap data) | Backend + Dashboard | UC-10 |
| Add daily activity bar chart (24h breakdown) | Dashboard | FR-8.9 |
| Add weekly activity bar chart (per weekday) | Dashboard | FR-8.10 |

**Deliverable:** Interactive heatmap and activity patterns fully integrated with date filtering.

---

### Phase 6: Data Export & Reporting

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Implement CSV export endpoint `GET /api/export/csv` for full user data | Backend | FR-13.1, UC-17 |
| Implement JSON export endpoint (structured) | Backend | FR-13.3 |
| Implement PDF report generation (using e.g., DinkToPdf or PuppeteerSharp) | Backend | FR-10.1, UC-12 |
| Create Reports page in dashboard with template selection (weekly, monthly, custom range) | Dashboard | FR-10.2, FR-10.5 |
| Add download buttons for each format, use one-time download tokens | Backend + Dashboard | FR-13.6 |
| Add basic scheduled report (optional: hangfire job to email weekly report) | Backend | FR-10.7, UC-19 |

**Deliverable:** User can export all data in CSV/JSON/PDF and generate weekly reports manually.

---

### Phase 7: Data Deletion & Privacy (GDPR)

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Implement API endpoint `DELETE /api/stats` (by date range) with password confirmation | Backend | FR-14.1, FR-14.5, UC-13 |
| Add UI in dashboard Settings → Data Management for deleting by date range | Dashboard | UC-13 |
| Implement full statistics reset endpoint (keep account) | Backend | FR-14.3 |
| Implement account deletion endpoint (soft delete + 30-day window) | Backend | FR-14.4, UC-18 |
| Add “Request data copy” endpoint (GDPR portability) | Backend | FR-17.3 |
| Add option to disable application context logging (if implemented) | Desktop + Backend | FR-17.2 |

**Deliverable:** Users can delete specific data, reset stats, and delete their account with confirmation workflow.

---

### Phase 8: Multi-Device Support

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Extend `Devices` table with `device_id`, `name`, `os`, `last_sync` | DB | FR-15.1 |
| Modify desktop client to generate/register a unique device ID on first login | Desktop + Backend | UC-14 |
| Extend sync API to accept `device_id` and store per-device key counts | Backend | FR-15.4 |
| Create API endpoint `GET /api/devices` to list user’s devices | Backend | FR-15.2 |
| Create Devices page in dashboard showing list, last sync, option to remove | Dashboard | FR-15.2, FR-15.3 |
| Add toggle switch to aggregate all devices (default) or show per-device stats | Dashboard | FR-15.5, UC-14 |

**Deliverable:** User can register multiple computers, view stats per device, and aggregate totals.

---

### Phase 9: Basic Administration (for resume)

| Task | Components | Related FRs / UCs |
|------|------------|--------------------|
| Seed an admin user role (`IsAdmin` flag in `Users` table) | DB, Backend | – |
| Create admin-only middleware that checks role | Backend | – |
| Build simple admin dashboard page (React) accessible only to admins | Dashboard | – |
| Implement user search and suspend/reactivate endpoints | Backend | FR-12.1, FR-12.3, FR-12.4, UC-15 |
| Add aggregate system stats endpoint (total users, total key events) | Backend | FR-12.5 |
| Build admin UI for user management (list, search, suspend) | Dashboard | UC-15 |
| Add audit log page for admins (login attempts, exports, suspensions) – basic | Backend + Dashboard | FR-18.1, FR-18.2, UC-20 |

**Deliverable:** Admin can view system stats, suspend users, and see recent audit events.

---

### Phase 10: Stretch Goals (optional / time permitting)

| Feature | Components | Related FRs / UCs |
|---------|------------|--------------------|
| Two-factor authentication (TOTP) | Backend + Dashboard | FR-1.6, UC-3 |
| Real-time live key display (SignalR + WebSocket) | Backend + Desktop + Dashboard | FR-16.1, FR-16.2, UC-16 |
| Typing speed alerts (real-time notifications) | Backend + Dashboard | FR-16.3 |
| Automatic sensitive field detection (password fields) | Desktop | FR-17.4 |
| Silent installation & auto-updates for enterprise | Desktop | FR-2.2, FR-2.3 |
| Full admin audit log (1-year retention, export) | Backend | FR-18.4 |
| Advanced conflict resolution (last-write-wins on sync) | Backend | FR-5.5 |
| Application context tracking (foreground window) | Desktop + Backend | FR-3.14, FR-9.6 |

---

## 4. Dependencies & Parallel Work

- **Phase 0** must be complete before anything else.
- **Phase 1** (auth) is prerequisite for Phase 2 (desktop needs login), Phase 4 (dashboard), and Phase 8 (multi-device).
- **Phase 2 & 3** can be built in parallel with Phase 1 if separate developers, but it’s safer to finish auth first.
- **Phase 4** depends on API endpoints created in Phase 3 (the sync storage). Can start frontend with mock data.
- **Phase 5** depends on Phase 4 completion.
- **Phase 6, 7, 8** depend on Phase 4 but can be done in any order after that.
- **Phase 9** can be done after Phase 1 and basic sync are stable.

---

## 5. Testing & Demo Milestones

| Milestone | Demonstration |
|-----------|---------------|
| Milestone 1 (end of Phase 3) | Desktop client counts keys, syncs to backend, data stored in SQL Server. |
| Milestone 2 (end of Phase 5) | Dashboard shows heatmap, trend chart, and filters by date. |
| Milestone 3 (end of Phase 7) | User can export, delete data, and delete account (GDPR). |
| Final | Multi-device, admin panel, and stretch goals (if any) all working. |

---

## 6. Recommended Branching Strategy

- `main` – stable, production-like
- `develop` – integration branch
- Feature branches: `feature/phase1-auth`, `feature/phase2-desktop-hook`, etc.
- Merge to develop after each phase is demo-ready.

---

*End of Implementation Order*