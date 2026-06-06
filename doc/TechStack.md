# Tech Stack

## Project Overview
A resume-level keyboard analytics platform with:
- A desktop client that captures keyboard usage only while the app is active
- A web dashboard for viewing stats and reports
- A backend API for authentication, synchronization, and data access
- A SQL Server database for persistent storage

## Recommended Stack

### 1. Desktop Client
**C# + WPF**
- Used for the Windows desktop app
- Good fit for keyboard event handling and local UI
- Easy integration with Windows features
- Solid choice for a resume project without adding too much complexity

**Local Storage: SQLite**
- Stores temporary or offline statistics locally
- Helps the app work even when the network is unavailable
- Simple to sync later with the main server

### 2. Backend API
**ASP.NET Core Web API**
- Handles user authentication
- Receives synced keyboard statistics from the desktop client
- Serves dashboard data to the web app
- Clean and professional choice for a Microsoft-based stack

### 3. Web Dashboard
**React + Vite**
- Used for the browser-based dashboard
- Fast setup and modern frontend workflow
- Good for charts, tables, filters, and reporting screens

**UI Library: Material UI or Tailwind CSS**
- Material UI if you want a polished, structured look
- Tailwind CSS if you want lightweight custom styling
- Either is fine for a resume project

**Charts: Recharts**
- Useful for line charts, bar charts, and usage graphs
- Simple enough for dashboard analytics

### 4. Database
**SQL Server**
- Main production database
- Stores users, devices, aggregated key counts, sync history, reports, and audit logs
- Strong choice for showing database design skills on a resume

### 5. Authentication
**JWT Authentication**
- Used for login sessions between frontend, backend, and desktop client
- Standard and widely understood in modern web applications

**Optional: Refresh Tokens**
- Useful if you want smoother login sessions
- Still reasonable for a resume project

### 6. Real-Time Updates
**SignalR**
- Optional but useful for live dashboard updates
- Good for showing real-time communication without making the project too large
- Can be used for sync status or live stats

### 7. Reporting and Export
**Server-side PDF generation**
- For downloadable reports

**CSV / JSON export**
- For backups and simple data sharing

### 8. Development Tools
- **Visual Studio** for backend and desktop development
- **VS Code** for frontend work
- **Git + GitHub** for version control
- **SQL Server Management Studio** for database management

## Suggested Final Stack
If the goal is to keep it resume-level and realistic, this is the stack I would pick:

- **Desktop:** C# + WPF + SQLite
- **Backend:** ASP.NET Core Web API
- **Frontend:** React + Vite
- **Database:** SQL Server
- **Auth:** JWT
- **Optional realtime:** SignalR
- **Charts:** Recharts

## Why This Stack Works
This stack is strong because it shows:
- Desktop development
- Web frontend development
- Backend API development
- Database design
- Authentication
- Real-time communication
- Data visualization

It is impressive enough for a resume, but still realistic enough to finish.

## Scope Control
To keep the project manageable, avoid adding too many extras at once. Good features to keep:
- Login and registration
- Desktop key counting while app is active
- Sync to SQL Server
- Dashboard charts
- Heatmap
- Reports

Features that can be left as stretch goals:
- Two-factor authentication
- Complex admin panel
- Scheduled reports
- Advanced audit logs
- Multi-device conflict resolution
