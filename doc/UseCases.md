# Use Case Document

## Keyboard Analytics System

**Version:** 2.0  
**Date:** 2026-06-05  
**Based on:** SRS v2.0

---

## 1. Actors

| Actor | Description |
|-------|-------------|
| User | Registered end-user who uses desktop client and web dashboard |
| Administrator | System admin who manages users and monitors system |
| Desktop Client | Automated actor representing the installed application |
| Backend API | Automated actor representing the server |

---

## 2. Use Cases

### UC-1: User Registration

| Actor | User |
|-------|------|
| Precondition | User has access to web dashboard. |
| Postcondition | User account created, email verified, user can log in. |

| User Action | System Response |
|-------------|-----------------|
| 1. User navigates to registration page and fills in username, email, password, accepts ToS. | 2. System validates input (unique username/email, password policy). |
| 3. User submits registration form. | 4. System creates account with status "unverified", sends verification email. |
| 5. User clicks verification link in email. | 6. System marks email as verified, redirects to login page. |

### UC-2: User Login (Web Dashboard)

| Actor | User |
|-------|------|
| Precondition | User has registered and verified email. |
| Postcondition | User authenticated and redirected to dashboard. |

| User Action | System Response |
|-------------|-----------------|
| 1. User enters username/email and password on login page. | 2. System validates credentials against database. |
| 3. User submits login form. | 4. System creates JWT access+refresh tokens, logs success (IP, user agent). |
| 5. User accesses dashboard. | 6. System grants access to dashboard with user-specific data. |

### UC-3: Two-Factor Authentication (2FA)

| Actor | User |
|-------|------|
| Precondition | User has enabled 2FA in profile settings. |
| Postcondition | User gains access after providing TOTP. |

| User Action | System Response |
|-------------|-----------------|
| 1. User provides valid credentials on login page. | 2. System prompts for TOTP code. |
| 3. User enters TOTP from authenticator app. | 4. System verifies code, completes authentication, issues tokens. |

### UC-4: Password Reset

| Actor | User |
|-------|------|
| Precondition | User forgot password and cannot log in. |
| Postcondition | User creates new password and regains access. |

| User Action | System Response |
|-------------|-----------------|
| 1. User clicks "Forgot Password" on login page, enters email. | 2. System validates email exists, sends password reset link with one-time token. |
| 3. User clicks link, enters new password (meets policy). | 4. System validates token, updates password, sends confirmation email. |

### UC-5: Desktop Client Installation and First-Time Setup

| Actor | User, Desktop Client |
|-------|----------------------|
| Precondition | User has an existing account. |
| Postcondition | Client installed, authenticated, and collecting keyboard events. |

| User Action | System Response |
|-------------|-----------------|
| 1. User downloads installer from website and runs it. | 2. System installs application (permissions: accessibility API). |
| 3. User launches client. | 4. Client displays login screen (device registration prompt). |
| 5. User enters credentials (or uses OAuth device code). | 6. Client sends credentials to backend API; receives access/refresh tokens. |
| 7. User grants OS-level accessibility permissions when prompted. | 8. Client starts system-wide keyboard hook, begins local event collection. |

### UC-6: Keyboard Event Collection (Desktop Client)

| Actor | Desktop Client |
|-------|----------------|
| Precondition | Client is authenticated and running with permissions. |
| Postcondition | Key press is recorded locally with timestamp and metadata. |

| User Action | System Response |
|-------------|-----------------|
| 1. User presses a key (e.g., 'A'). | 2. Client captures key scan code, logical key, modifier state, timestamp. |
| 3. (Repeat) User holds key. | 4. Client applies debounce (50ms) to ignore auto-repeat; counts only first press. |
| 5. User releases key. | 6. Client increments per-key counter in local encrypted SQLite database. |
| 7. (Optional) User types in sensitive field (password). | 8. Client detects secure input context (OS hint) and discards those events. |

### UC-7: Automatic Data Synchronization

| Actor | Desktop Client, Backend API |
|-------|----------------------------|
| Precondition | Client has unsynchronized local data and network connectivity. |
| Postcondition | Local data uploaded, merged with remote, sync status updated. |

| User Action | System Response |
|-------------|-----------------|
| 1. (Automatic) Timer triggers (every 5 min). | 2. Client gathers pending events (since last sync), creates JSON payload. |
| 3. Client sends payload to API endpoint with JWT. | 4. API verifies token, validates payload structure. |
| 5. (Conflict) Some events already exist remotely. | 6. API merges using additive strategy (sum counts per key+time bucket). |
| 7. API stores merged data in time-series DB. | 8. API returns success with last sync timestamp. |
| 9. Client updates local last_sync_time, clears synced events. | 10. Client displays "Sync successful" in tray menu. |

### UC-8: Manual Synchronization

| Actor | User, Desktop Client |
|-------|----------------------|
| Precondition | Client is running and authenticated. |
| Postcondition | Synchronization triggered immediately. |

| User Action | System Response |
|-------------|-----------------|
| 1. User right-clicks system tray icon, selects "Sync Now". | 2. Client initiates sync process (same as UC-7 steps 2-9). |
| 3. User views sync status in tooltip. | 4. Client updates status to "Syncing...", then "Last sync: just now". |

### UC-9: View Dashboard – Total Key Presses

| Actor | User (Web) |
|-------|------------|
| Precondition | User logged into dashboard. |
| Postcondition | User sees total key counts aggregated over selected period. |

| User Action | System Response |
|-------------|-----------------|
| 1. User navigates to Dashboard home. | 2. System loads lifetime total, today's total, this month's total from API. |
| 3. User hovers over "Today" card. | 4. System displays tooltip with hourly breakdown. |
| 5. User clicks "Refresh". | 6. System fetches latest data and updates displayed numbers. |

### UC-10: View Keyboard Heatmap

| Actor | User (Web) |
|-------|------------|
| Precondition | User logged in, data exists for selected period. |
| Postcondition | Interactive heatmap displayed. |

| User Action | System Response |
|-------------|-----------------|
| 1. User selects "Heatmap" tab. | 2. System fetches per-key counts for default period (this month). |
| 3. System renders SVG keyboard, colors keys from light yellow (low) to dark red (high). |
| 4. User clicks a key on heatmap (e.g., 'Space'). | 5. System displays modal with exact count and historical trend for that key. |
| 6. User selects date filter "Last 7 days". | 7. System re-fetches data, updates heatmap colors dynamically. |

### UC-11: Apply Date Range Filter

| Actor | User (Web) |
|-------|------------|
| Precondition | User on any statistics page. |
| Postcondition | All charts and tables update to show filtered data. |

| User Action | System Response |
|-------------|-----------------|
| 1. User clicks date range picker. | 2. System displays preset options (Today, This Week, This Month, Custom). |
| 3. User selects "Custom" and chooses start date 2026-06-01, end date 2026-06-05. | 4. System validates range (max 1 year). |
| 5. User applies filter. | 6. System sends API request with `?from=2026-06-01&to=2026-06-05`, updates all visualizations. |
| 7. User saves filter as "June first week". | 8. System stores named filter in user preferences. |

### UC-12: Generate and Download Weekly Report

| Actor | User (Web) |
|-------|------------|
| Precondition | User has sufficient data for the past week. |
| Postcondition | Report file (PDF/CSV/JSON) downloaded. |

| User Action | System Response |
|-------------|-----------------|
| 1. User navigates to Reports page. | 2. System displays report templates (Daily, Weekly, Monthly, Custom). |
| 3. User selects "Weekly Report" for previous week (Monday–Sunday). | 4. System generates report summary (total keys, top 5 keys, daily activity chart). |
| 5. User chooses format "PDF" and clicks "Download". | 6. System compiles report into PDF, returns as downloadable file with one-time token. |
| 7. User saves file. | 8. System logs export event (user, timestamp, report type). |

### UC-13: Delete User Data (Date Range)

| Actor | User (Web) |
|-------|------------|
| Precondition | User is logged in, data exists in selected range. |
| Postcondition | Specified statistics are permanently removed. |

| User Action | System Response |
|-------------|-----------------|
| 1. User goes to Settings → Data Management. | 2. System displays options: Delete by Date, Delete by Key, Full Reset. |
| 3. User selects "Delete by Date", enters range 2026-05-01 to 2026-05-15. | 4. System shows confirmation dialog with impact (estimated 1,200 key events). |
| 5. User confirms by entering password and clicking "Permanently Delete". | 6. System verifies password, soft-deletes records (mark as deleted). |
| 7. User receives confirmation email. | 8. System schedules hard deletion after 30-day recovery window. |

### UC-14: Multi-Device – Register Second Computer

| Actor | User, Desktop Client |
|-------|----------------------|
| Precondition | User has one registered device, installs client on a second computer. |
| Postcondition | Second device appears in dashboard, data collected separately. |

| User Action | System Response |
|-------------|-----------------|
| 1. User installs client on laptop, logs in with same credentials. | 2. Backend API generates new device ID, stores device info (OS, hostname, version). |
| 3. User opens dashboard → Devices. | 4. System lists both devices: "Desktop-PC" and "Laptop" with last sync times. |
| 5. User clicks "View Device Stats" for Laptop. | 6. System shows per-device key counts (only from that device). |
| 7. User toggles "Aggregate all devices". | 8. System combines statistics from both devices. |

### UC-15: Administrator – Suspend User Account

| Actor | Administrator |
|-------|---------------|
| Precondition | Admin logged into admin panel, user exists. |
| Postcondition | User account suspended, cannot log in or sync. |

| User Action | System Response |
|-------------|-----------------|
| 1. Admin searches for user by email "john@example.com". | 2. System returns user profile. |
| 3. Admin clicks "Suspend Account", enters reason "Violation of ToS". | 4. System prompts for confirmation. |
| 5. Admin confirms. | 6. System marks user as suspended, invalidates all tokens. |
| 7. System logs action in audit log. | 8. System sends email notification to user (optional). |

### UC-16: Real-Time Live Key Display

| Actor | User (Web) |
|-------|------------|
| Precondition | User has desktop client running and authenticated, dashboard open. |
| Postcondition | User sees last key pressed in real time. |

| User Action | System Response |
|-------------|-----------------|
| 1. User navigates to "Live View" page. | 2. System establishes WebSocket connection to backend. |
| 3. Desktop client sends a key event (e.g., 'Enter'). | 4. Backend forwards event via WebSocket to dashboard. |
| 5. (Real-time) User watches the screen. | 6. Dashboard displays "Last key: Enter" and increments live counter. |
| 7. User presses 200 keys in one minute. | 8. System triggers speed alert notification (if user enabled >200 keys/min). |

### UC-17: Export All Data in JSON for Backup

| Actor | User (Web) |
|-------|------------|
| Precondition | User has data. |
| Postcondition | JSON file downloaded containing all statistics. |

| User Action | System Response |
|-------------|-----------------|
| 1. User goes to Settings → Export. | 2. System lists export formats: CSV, Excel, JSON, PDF. |
| 3. User selects "JSON" and clicks "Export Full History". | 4. System queries database for all user key events (aggregated). |
| 5. System generates JSON structure, compresses if >10MB. | 6. System creates one-time download link (expires 1 hour). |
| 7. User clicks download link. | 8. System serves file, logs export event. |

### UC-18: Request Account Deletion (GDPR)

| Actor | User (Web) |
|-------|------------|
| Precondition | User wants to delete entire account. |
| Postcondition | Account and all associated data scheduled for deletion. |

| User Action | System Response |
|-------------|-----------------|
| 1. User goes to Settings → Danger Zone → Delete Account. | 2. System displays warning: "All data will be permanently lost after 30 days." |
| 3. User enters password and checks confirmation box. | 4. System verifies password, creates deletion request. |
| 5. User clicks "Request Immediate Deletion". | 6. System sends verification email with "Confirm Deletion" link. |
| 7. User clicks link. | 8. System soft-deletes account, sets deletion date +30 days, logs user out. |
| 9. (After 30 days) System permanently erases data. | 10. System sends final confirmation email. |

### UC-19: Schedule Automatic Report (Weekly)

| Actor | User (Web) |
|-------|------------|
| Precondition | User has data. |
| Postcondition | Report generated and emailed every week automatically. |

| User Action | System Response |
|-------------|-----------------|
| 1. User navigates to Reports → Scheduled. | 2. System shows existing schedules (none initially). |
| 3. User clicks "New Schedule", selects Weekly, format PDF, recipients (user's email). | 4. System validates schedule. |
| 5. User sets day = Monday, time = 08:00 UTC, clicks Save. | 6. System stores cron-like schedule. |
| 7. (Every Monday 08:00 UTC) System generates report for previous week. | 8. System emails PDF to user, stores copy in dashboard "My Reports". |
| 9. User receives email, clicks download link. | 10. System serves report file. |

### UC-20: Administrator View Audit Log

| Actor | Administrator |
|-------|---------------|
| Precondition | Admin logged in. |
| Postcondition | Admin views filtered audit trail. |

| User Action | System Response |
|-------------|-----------------|
| 1. Admin clicks "Audit Log" in admin panel. | 2. System displays last 100 events (timestamp, actor, action, IP, target). |
| 3. Admin filters by action type "USER_SUSPEND". | 4. System returns only suspension events. |
| 5. Admin filters by date range last 7 days. | 6. System updates table with filtered results. |
| 7. Admin clicks "Export Audit Log (CSV)". | 8. System generates CSV with audit data, initiates download. |

---

## 3. Use Case Coverage Map (vs SRS FRs)

| Use Case ID | Related FRs |
|-------------|-------------|
| UC-1 | FR-1.1, FR-1.2, FR-1.3, FR-1.4 |
| UC-2 | FR-1.5, FR-1.7, FR-1.13, FR-18.1 |
| UC-3 | FR-1.6 |
| UC-4 | FR-1.10 |
| UC-5 | FR-2.1, FR-2.2, FR-2.4, FR-2.5, FR-2.6 |
| UC-6 | FR-3.1 to FR-3.15, FR-17.4 |
| UC-7 | FR-5.1, FR-5.4, FR-5.5, FR-5.6 |
| UC-8 | FR-5.2, FR-5.3 |
| UC-9 | FR-8.2, FR-8.3 |
| UC-10 | FR-8.6, FR-9.1, FR-9.2 |
| UC-11 | FR-9.1, FR-9.4 |
| UC-12 | FR-10.1, FR-10.3, FR-10.6, FR-13.4 |
| UC-13 | FR-14.1, FR-14.5, FR-14.6 |
| UC-14 | FR-15.1, FR-15.2, FR-15.4, FR-15.5 |
| UC-15 | FR-12.3, FR-12.7, FR-18.3 |
| UC-16 | FR-16.1, FR-16.2, FR-16.3 |
| UC-17 | FR-13.3, FR-13.6, FR-18.2 |
| UC-18 | FR-14.4, FR-14.6, FR-17.3 |
| UC-19 | FR-10.7, FR-11.4 |
| UC-20 | FR-12.7, FR-18.4 |

---

*End of Use Case Document*