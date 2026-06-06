# Software Requirements Specification (SRS)

## Keyboard Analytics System

**Version:** 2.0 (Detailed)  
**Date:** 2026-06-05  
**Document Status:** Draft

---

## 1. Introduction

### 1.1 Purpose
This document specifies the functional requirements for the Keyboard Analytics System – a cross-platform solution that collects, stores, analyzes, and visualizes keyboard usage statistics across multiple devices.

### 1.2 Scope
The system consists of:
- A **desktop client** (Windows, macOS, Linux) for event collection
- A **backend API** (RESTful/GraphQL) for data processing and storage
- A **web dashboard** for visualization, reporting, and administration
- A **database** (relational + time-series) for persistent storage

### 1.3 Definitions
- **Key Event:** A single key press (down + up) counted as one occurrence.
- **Modifier Key:** Shift, Ctrl, Alt, Win/Cmd, Fn.
- **Synchronization:** The process of uploading local statistics to the backend.

---

## 2. Non-Functional Requirements (NFR)

| NFR_ID | Category | Description |
|--------|----------|-------------|
| NFR-1 | Performance | The desktop client shall consume less than 2% CPU on average and <50 MB RAM. |
| NFR-2 | Performance | The API shall respond to 95% of requests within 300 ms under normal load. |
| NFR-3 | Performance | Dashboard initial load shall complete within 2 seconds on a broadband connection. |
| NFR-4 | Security | All authentication tokens shall expire after 24 hours of inactivity. |
| NFR-5 | Security | All client-server communication shall use TLS 1.3. |
| NFR-6 | Security | Passwords shall be hashed using bcrypt (cost factor 12). |
| NFR-7 | Availability | The backend API shall have 99.9% uptime. |
| NFR-8 | Scalability | The system shall support at least 100,000 concurrent users. |
| NFR-9 | Usability | The dashboard shall be WCAG 2.1 AA compliant. |
| NFR-10 | Maintainability | The system shall include API documentation (OpenAPI 3.0). |
| NFR-11 | Compliance | The system shall comply with GDPR and CCPA data privacy requirements. |
| NFR-12 | Backup | Database backups shall occur daily with 30-day retention. |
| NFR-13 | Localization | The dashboard and client UI shall support English and Spanish initially. |

---

## 3. Functional Requirements (FR)

### FR-1 User Management

| FR_ID | Description |
|-------|-------------|
| FR-1.1 | The system shall allow a new user to create an account using username, email, password, and acceptance of Terms of Service. |
| FR-1.2 | The system shall enforce a password policy: minimum 8 characters, including at least one uppercase, one lowercase, one digit, and one special character. |
| FR-1.3 | The system shall verify email addresses via a confirmation link sent after registration. |
| FR-1.4 | The system shall prevent duplicate usernames and email addresses. |
| FR-1.5 | The system shall authenticate registered users using username/email and password before granting access. |
| FR-1.6 | The system shall support two-factor authentication (TOTP or SMS) for enhanced security. |
| FR-1.7 | The system shall allow users to log in using valid credentials. |
| FR-1.8 | The system shall lock an account after 5 failed login attempts within 15 minutes (unlock via email or admin). |
| FR-1.9 | The system shall allow users to log out, terminating their active session on both client and web. |
| FR-1.10 | The system shall allow users to reset forgotten passwords via email-based verification. |
| FR-1.11 | The system shall allow users to view and modify their profile information (name, email, password, preferences). |
| FR-1.12 | The system shall allow users to enable/disable email notifications. |
| FR-1.13 | The system shall maintain a session timeout of 12 hours on web and indefinite on client (with token refresh). |

### FR-2 Desktop Client Installation

| FR_ID | Description |
|-------|-------------|
| FR-2.1 | The system shall provide an installable desktop application for Windows (MSI/EXE), macOS (DMG/PKG), and Linux (DEB/RPM/AppImage). |
| FR-2.2 | The desktop application shall check for updates automatically and prompt the user to install. |
| FR-2.3 | The desktop application shall support silent installation for enterprise deployment. |
| FR-2.4 | The desktop application shall guide users through first-time setup: login, permission requests (accessibility API), and privacy acknowledgment. |
| FR-2.5 | The desktop client shall authenticate users against the backend API using OAuth2 (password grant or device code flow). |
| FR-2.6 | The desktop client shall maintain authenticated sessions between launches using a securely stored refresh token. |
| FR-2.7 | The desktop client shall allow the user to manually log out, clearing all locally stored tokens. |
| FR-2.8 | The desktop client shall display its current version and build date in the settings panel. |

### FR-3 Keyboard Event Collection

| FR_ID | Description |
|-------|-------------|
| FR-3.1 | The desktop application shall detect all keyboard key-press events system-wide (including in other applications). |
| FR-3.2 | The desktop application shall distinguish between key down, key hold (repeat), and key up events, counting only intentional presses (ignore auto-repeat after configurable debounce – default 50ms). |
| FR-3.3 | The desktop application shall identify the specific physical key scanned code (hardware-independent) and logical key (considering current keyboard layout). |
| FR-3.4 | The desktop application shall increment the count associated with the detected key for both press and release (press counts as usage). |
| FR-3.5 | The desktop application shall track modifier keys: Shift (left/right separately), Ctrl, Alt, Win (Windows)/Cmd (macOS), Fn. |
| FR-3.6 | The desktop application shall track function keys F1 through F24. |
| FR-3.7 | The desktop application shall track navigation keys: Arrow Keys (Up, Down, Left, Right), Home, End, Page Up, Page Down. |
| FR-3.8 | The desktop application shall track special keys: Enter, Backspace, Delete, Tab, Escape, Spacebar, Insert, Print Screen, Scroll Lock, Pause. |
| FR-3.9 | The desktop application shall track numeric keys: top row (0-9) and numpad keys (0-9, ., +, -, *, /, Enter). |
| FR-3.10 | The desktop application shall track all alphabetic keys (A-Z) preserving case sensitivity (Shift+A vs a). |
| FR-3.11 | The desktop application shall track symbol keys: ``~!@#$%^&*()_+{}|:"<>?`` and regional variants. |
| FR-3.12 | The desktop application shall record a timestamp (UTC) for every key event. |
| FR-3.13 | The desktop application shall differentiate between left and right modifier keys (LShift vs RShift, LCtrl vs RCtrl). |
| FR-3.14 | The desktop application shall detect and log the active foreground application/window title for context (optional, user-controlled). |
| FR-3.15 | The desktop application shall provide a pause/resume feature to temporarily stop collection (e.g., during sensitive data entry). |

### FR-4 Local Data Management

| FR_ID | Description |
|-------|-------------|
| FR-4.1 | The desktop application shall maintain a local SQLite or LevelDB cache of collected statistics, including per-key, per-hour, and per-day aggregates. |
| FR-4.2 | The local database shall be encrypted (AES-256) using a key derived from the user's password or a hardware-backed keystore. |
| FR-4.3 | The desktop application shall continue collecting statistics when disconnected from the network (offline operation). |
| FR-4.4 | The desktop application shall retrieve locally stored statistics for display in a local-only mode. |
| FR-4.5 | The desktop application shall update existing local statistics incrementally (no duplicate events). |
| FR-4.6 | The local cache shall retain up to 90 days of detailed event data by default (configurable). |
| FR-4.7 | The desktop application shall automatically prune local data older than the retention period. |
| FR-4.8 | The desktop application shall allow the user to manually clear local cache from settings. |

### FR-5 Data Synchronization

| FR_ID | Description |
|-------|-------------|
| FR-5.1 | The desktop application shall automatically synchronize collected statistics with the backend every 5 minutes (configurable interval). |
| FR-5.2 | The user shall be able to initiate manual synchronization from the desktop client's system tray menu. |
| FR-5.3 | The system shall display synchronization status (Syncing, Last Sync Time, Pending Events, Failed) in both client and dashboard. |
| FR-5.4 | During synchronization, the system shall merge local and remote statistics using an additive strategy (summing counts per key and timestamp bucket). |
| FR-5.5 | In case of timestamp conflict, the system shall accept the event with the latest timestamp (last-write-wins). |
| FR-5.6 | The system shall retry failed synchronization attempts with exponential backoff (initial 30s, max 1 hour). |
| FR-5.7 | The desktop application shall queue events when offline and sync them in chronological order upon reconnection. |
| FR-5.8 | The system shall notify the user of sync failures via a system tray notification with an option to retry. |

### FR-6 Backend API

| FR_ID | Description |
|-------|-------------|
| FR-6.1 | The API shall receive keyboard statistics from desktop clients via JSON payloads over HTTPS. |
| FR-6.2 | The API shall verify user identity using JWT tokens (access + refresh) before processing any request. |
| FR-6.3 | The API shall store received statistics in a time-series database (e.g., InfluxDB or TimescaleDB). |
| FR-6.4 | The API shall update previously stored statistics by adding new counts to existing buckets (upsert). |
| FR-6.5 | The API shall provide stored statistics upon request, supporting aggregation by hour, day, week, month, year. |
| FR-6.6 | The API shall support filtered data retrieval by date range, key(s), device ID, and application context. |
| FR-6.7 | The API shall rate-limit requests to 100 requests per minute per user. |
| FR-6.8 | The API shall log all data access events for audit purposes (user, timestamp, resource, action). |

### FR-7 Keyboard Statistics Management

| FR_ID | Description |
|-------|-------------|
| FR-7.1 | The system shall maintain total key-press counts across all keys for each user. |
| FR-7.2 | The system shall maintain counts for each individual key (by key code and display name). |
| FR-7.3 | The system shall maintain daily key statistics (UTC date buckets). |
| FR-7.4 | The system shall maintain weekly key statistics (ISO week numbers). |
| FR-7.5 | The system shall maintain monthly key statistics (calendar months). |
| FR-7.6 | The system shall maintain lifetime cumulative statistics since account creation. |
| FR-7.7 | The system shall compute average keys per day, peak usage hour, and most active weekday per user. |
| FR-7.8 | The system shall detect and flag anomalous usage patterns (e.g., sudden 10x increase) for user review. |

### FR-8 Dashboard Visualization

| FR_ID | Description |
|-------|-------------|
| FR-8.1 | The web application shall provide a statistics dashboard accessible after login. |
| FR-8.2 | The dashboard shall display total recorded key presses (lifetime, this month, today). |
| FR-8.3 | The dashboard shall display counts for each tracked key in sortable tables. |
| FR-8.4 | The dashboard shall display the top 10 most frequently used keys. |
| FR-8.5 | The dashboard shall display the least used keys (bottom 5). |
| FR-8.6 | The dashboard shall display a visual keyboard heatmap (color intensity by key usage). |
| FR-8.7 | The dashboard shall display a pie/bar chart for key usage distribution by category (letters, numbers, modifiers, etc.). |
| FR-8.8 | The dashboard shall display historical trend lines (line chart) for total key presses over time. |
| FR-8.9 | The dashboard shall display a daily activity graph (bar chart with 24-hour breakdown). |
| FR-8.10 | The dashboard shall display a weekly activity graph (stacked bar per weekday). |
| FR-8.11 | The dashboard shall display a monthly activity graph (line or bar per month). |
| FR-8.12 | The dashboard shall allow toggling between dark/light themes. |
| FR-8.13 | The dashboard shall be responsive (mobile, tablet, desktop). |

### FR-9 Search and Filtering

| FR_ID | Description |
|-------|-------------|
| FR-9.1 | The dashboard shall allow filtering statistics by date range (presets: today, this week, this month, custom). |
| FR-9.2 | The dashboard shall allow filtering by specific keys (multi-select). |
| FR-9.3 | The dashboard shall allow filtering by activity level (e.g., keys with count >100). |
| FR-9.4 | The dashboard shall allow creation and saving of custom filter combinations (named filters). |
| FR-9.5 | The dashboard shall allow filtering by device (if multiple devices are registered). |
| FR-9.6 | The dashboard shall allow filtering by foreground application (if context tracking enabled). |

### FR-10 Reporting

| FR_ID | Description |
|-------|-------------|
| FR-10.1 | The system shall generate usage reports in PDF, HTML, or CSV format. |
| FR-10.2 | The system shall generate daily reports summarizing key activity for the previous calendar day. |
| FR-10.3 | The system shall generate weekly reports (Monday–Sunday) with comparisons to prior week. |
| FR-10.4 | The system shall generate monthly reports with month-over-month trends. |
| FR-10.5 | The system shall generate custom reports for user-selected date ranges. |
| FR-10.6 | The system shall allow report downloads as a single ZIP file containing all formats (PDF, CSV, JSON). |
| FR-10.7 | The system shall allow scheduled automatic report generation (daily, weekly, monthly) emailed to the user. |

### FR-11 Notifications

| FR_ID | Description |
|-------|-------------|
| FR-11.1 | The system shall notify users upon successful synchronization (optional, default off). |
| FR-11.2 | The system shall notify users when synchronization fails (after 3 consecutive retries). |
| FR-11.3 | The system shall notify users of successful login from a new device (email or push). |
| FR-11.4 | The system shall notify users when a scheduled report is ready for download. |
| FR-11.5 | The system shall notify users when storage usage exceeds 90% of quota (if applicable). |
| FR-11.6 | The system shall allow users to configure notification preferences per channel (email, desktop, in-app). |

### FR-12 Administration

| FR_ID | Description |
|-------|-------------|
| FR-12.1 | The administrator shall manage user accounts (view, edit, delete, suspend, reactivate). |
| FR-12.2 | The administrator shall search for users by username, email, or device ID. |
| FR-12.3 | The administrator shall suspend user accounts (prevents login and sync). |
| FR-12.4 | The administrator shall reactivate suspended accounts. |
| FR-12.5 | The administrator shall view aggregate system statistics (total users, total key events, active devices). |
| FR-12.6 | The administrator shall monitor connected desktop clients (last seen, version, sync status). |
| FR-12.7 | The administrator shall view system activity logs (login attempts, data exports, admin actions) with filtering. |
| FR-12.8 | The administrator shall set global system limits (max users, max keys per day per user). |

### FR-13 Data Export

| FR_ID | Description |
|-------|-------------|
| FR-13.1 | The system shall export statistics to CSV format (UTF-8 encoding). |
| FR-13.2 | The system shall export statistics to Excel format (.xlsx) with multiple sheets (summary, daily, per-key). |
| FR-13.3 | The system shall export statistics to JSON format (structured for programmatic use). |
| FR-13.4 | The system shall export statistics to PDF format (printable, with charts and tables). |
| FR-13.5 | The system shall allow export of raw event logs (detailed timestamps per key) for power users. |
| FR-13.6 | All exports shall be protected with a user-specific one-time download token (expires in 1 hour). |

### FR-14 Data Deletion

| FR_ID | Description |
|-------|-------------|
| FR-14.1 | The user shall be able to delete collected statistics for a specific date range. |
| FR-14.2 | The user shall be able to delete statistics by key (e.g., remove all counts for a specific key). |
| FR-14.3 | The user shall be able to perform a full statistics reset (clear all collected data but keep account). |
| FR-14.4 | The user shall be able to delete their entire account, including all statistics and profile data (GDPR compliance). |
| FR-14.5 | Deletion operations shall require confirmation with password entry. |
| FR-14.6 | The system shall permanently erase data within 30 days of deletion request (soft delete with recovery window). |

### FR-15 Multi-Device Support

| FR_ID | Description |
|-------|-------------|
| FR-15.1 | The system shall register multiple devices per user (each desktop client installation). |
| FR-15.2 | The system shall display a list of registered devices in the dashboard with name, last sync, OS, and version. |
| FR-15.3 | The user shall be able to remove (unregister) a device from the dashboard. |
| FR-15.4 | The system shall maintain statistics for individual devices (per-device key counts). |
| FR-15.5 | The system shall aggregate statistics across all devices for holistic views. |
| FR-15.6 | The user shall be able to set a primary device (used as baseline for trends). |
| FR-15.7 | The user shall be able to merge data from two devices if duplicates are detected. |

### FR-16 Real-time Features (New)

| FR_ID | Description |
|-------|-------------|
| FR-16.1 | The dashboard shall display live key counts (WebSocket or SSE) for the currently active device. |
| FR-16.2 | The dashboard shall show a real-time "last key pressed" indicator. |
| FR-16.3 | The system shall allow the user to set real-time typing speed alerts (e.g., notify if >200 keys/minute). |

### FR-17 Privacy and Anonymization (New)

| FR_ID | Description |
|-------|-------------|
| FR-17.1 | The system shall allow users to anonymize their data before export (replace usernames with random IDs). |
| FR-17.2 | The system shall provide an option to disable application context logging entirely. |
| FR-17.3 | The system shall allow users to request a complete copy of their personal data (GDPR data portability). |
| FR-17.4 | The system shall automatically remove sensitive key sequences (passwords, credit card numbers) by filtering out events occurring within password fields (via OS accessibility hint). |

### FR-18 Audit Logging (New)

| FR_ID | Description |
|-------|-------------|
| FR-18.1 | The system shall log all user authentication events (success, failure, IP address, user agent). |
| FR-18.2 | The system shall log all data export and deletion events. |
| FR-18.3 | The system shall log all administrator actions (user suspension, data view, system configuration changes). |
| FR-18.4 | Audit logs shall be retained for 1 year and be accessible to administrators via a dedicated interface. |

### FR-19 Help & Documentation

| FR_ID | Description |
|-------|-------------|
| FR-19.1 | The system shall provide in-app help documentation for dashboard and desktop client. |
| FR-19.2 | The system shall include a guided tour for first-time dashboard users. |
| FR-19.3 | The system shall display tooltips for key dashboard elements. |
| FR-19.4 | The system shall provide a FAQ section and a way to submit support tickets. |

---

## 4. Summary of Requirements

| Category | Number of FRs |
|----------|---------------|
| User Management | 13 |
| Desktop Client Installation | 8 |
| Keyboard Event Collection | 15 |
| Local Data Management | 8 |
| Data Synchronization | 8 |
| Backend API | 8 |
| Keyboard Statistics Management | 8 |
| Dashboard Visualization | 13 |
| Search and Filtering | 6 |
| Reporting | 7 |
| Notifications | 6 |
| Administration | 8 |
| Data Export | 6 |
| Data Deletion | 6 |
| Multi-Device Support | 7 |
| Real-time Features | 3 |
| Privacy and Anonymization | 4 |
| Audit Logging | 4 |
| Help & Documentation | 4 |
| **Total** | **141** |

---

*End of SRS Document*