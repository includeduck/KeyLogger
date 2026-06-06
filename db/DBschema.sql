-- ============================================================
-- Database Schema: Keyboard Analytics System (T-SQL / SQL Server)
-- Version: 1.0
-- Based on SRS v2.0, Use Cases, TechStack
-- ============================================================

IF DB_ID(N'KeyMapperDb') IS NULL
BEGIN
    CREATE DATABASE KeyMapperDb;
END;
GO

USE KeyMapperDb;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

-- ============================================================
-- 1. TABLES
-- ============================================================

-- 1.1 Users (authentication, profile, account status)
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,        -- bcrypt hash
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    EmailVerificationToken NVARCHAR(255) NULL,
    EmailVerificationTokenExpiry DATETIME2 NULL,
    PasswordResetToken NVARCHAR(255) NULL,
    PasswordResetTokenExpiry DATETIME2 NULL,
    IsSuspended BIT NOT NULL DEFAULT 0,
    SuspensionReason NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,            -- soft delete for GDPR (30-day recovery)
    DeletionRequestDate DATETIME2 NULL,
    PermanentDeletionDate DATETIME2 NULL,        -- = DeletionRequestDate + 30 days
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret NVARCHAR(255) NULL,          -- TOTP secret (encrypted)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2 NULL
);

-- 1.2 User Preferences (notifications, theme, privacy)
CREATE TABLE dbo.UserPreferences (
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    Theme NVARCHAR(20) NOT NULL DEFAULT 'light',   -- 'light' or 'dark'
    EmailNotificationsEnabled BIT NOT NULL DEFAULT 1,
    DesktopNotificationsEnabled BIT NOT NULL DEFAULT 1,
    SyncSuccessNotification BIT NOT NULL DEFAULT 0,
    SyncFailureNotification BIT NOT NULL DEFAULT 1,
    AppContextLoggingEnabled BIT NOT NULL DEFAULT 0,  -- FR-17.2
    Language NVARCHAR(10) NOT NULL DEFAULT 'en',      -- 'en', 'es'
    PRIMARY KEY (UserId)
);

-- 1.3 Refresh Tokens (for desktop client & web session persistence)
CREATE TABLE dbo.RefreshTokens (
    RefreshTokenId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    DeviceId NVARCHAR(100) NULL,                -- optional device identifier
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 1.4 Devices (multiple devices per user)
CREATE TABLE dbo.Devices (
    DeviceId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DeviceUniqueId NVARCHAR(200) NOT NULL,       -- client-generated UUID
    DeviceName NVARCHAR(100) NOT NULL,           -- e.g., "Work Laptop"
    OperatingSystem NVARCHAR(50) NOT NULL,       -- Windows, macOS, Linux
    ClientVersion NVARCHAR(20) NOT NULL,
    LastSyncAt DATETIME2 NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    RegisteredAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_DeviceUniqueId UNIQUE (UserId, DeviceUniqueId)
);

-- 1.5 Key Counts – Daily Aggregation (time-series friendly)
CREATE TABLE dbo.KeyCountsDaily (
    KeyCountId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DeviceId INT NOT NULL REFERENCES dbo.Devices(DeviceId),
    KeyCode NVARCHAR(50) NOT NULL,               -- e.g., "Key.A", "Space", "LShift"
    KeyDisplayName NVARCHAR(50) NOT NULL,        -- human-readable "A", "Space", "Left Shift"
    EventDate DATE NOT NULL,                     -- UTC date bucket
    PressCount INT NOT NULL CHECK (PressCount >= 0),
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_KeyCountsDaily UNIQUE (UserId, DeviceId, KeyCode, EventDate)
);

-- 1.6 Key Counts – Hourly Aggregation (for detailed activity charts)
CREATE TABLE dbo.KeyCountsHourly (
    KeyCountHourlyId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DeviceId INT NOT NULL REFERENCES dbo.Devices(DeviceId),
    KeyCode NVARCHAR(50) NOT NULL,
    KeyDisplayName NVARCHAR(50) NOT NULL,
    EventHour DATETIME2 NOT NULL,                -- truncated hour (UTC)
    PressCount INT NOT NULL CHECK (PressCount >= 0),
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_KeyCountsHourly UNIQUE (UserId, DeviceId, KeyCode, EventHour)
);

-- 1.7 Sync Logs (track sync attempts between desktop client and backend)
CREATE TABLE dbo.SyncLogs (
    SyncLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DeviceId INT NOT NULL REFERENCES dbo.Devices(DeviceId),
    StartedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL,                -- 'Pending', 'Success', 'Failed'
    EventsSynced INT NULL,
    ErrorMessage NVARCHAR(500) NULL,
    RetryCount INT NOT NULL DEFAULT 0
);

-- 1.8 Audit Logs (FR-18: authentication, exports, admin actions)
CREATE TABLE dbo.AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL REFERENCES dbo.Users(UserId) ON DELETE SET NULL,  -- can be null for system actions
    ActorType NVARCHAR(20) NOT NULL,             -- 'User', 'Admin', 'System'
    Action NVARCHAR(100) NOT NULL,               -- e.g., 'LOGIN_SUCCESS', 'EXPORT_DATA', 'USER_SUSPEND'
    TargetType NVARCHAR(50) NULL,                -- 'User', 'Device', 'KeyCount'
    TargetId NVARCHAR(200) NULL,                 -- identifier like username or device ID
    IPAddress NVARCHAR(45) NULL,                 -- IPv4/IPv6
    UserAgent NVARCHAR(300) NULL,
    Details NVARCHAR(MAX) NULL,                  -- JSON extra info
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 1.9 Report Schedules (FR-10.7)
CREATE TABLE dbo.ReportSchedules (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    ScheduleName NVARCHAR(100) NOT NULL,
    ReportType NVARCHAR(20) NOT NULL,            -- 'Daily', 'Weekly', 'Monthly'
    Format NVARCHAR(10) NOT NULL,                -- 'PDF', 'CSV', 'JSON'
    CronExpression NVARCHAR(100) NOT NULL,       -- e.g., '0 8 * * 1' for Monday 8am UTC
    RecipientEmail NVARCHAR(255) NOT NULL,       -- usually user's email
    IsActive BIT NOT NULL DEFAULT 1,
    LastGeneratedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 1.10 User Data Export Requests (GDPR data portability – FR-17.3)
CREATE TABLE dbo.DataExportRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    Format NVARCHAR(10) NOT NULL,                -- 'JSON', 'CSV', 'PDF'
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Completed', 'Failed'
    FilePath NVARCHAR(500) NULL,
    DownloadToken UNIQUEIDENTIFIER NULL,         -- one-time token (FR-13.6)
    TokenExpiry DATETIME2 NULL,
    RequestedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt DATETIME2 NULL
);

-- ============================================================
-- 2. INDEXES (performance)
-- ============================================================

-- Users: fast lookup by email or username
CREATE INDEX IX_Users_Email ON dbo.Users(Email) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_Username ON dbo.Users(Username) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_IsSuspended ON dbo.Users(IsSuspended);
CREATE INDEX IX_Users_DeletionDate ON dbo.Users(PermanentDeletionDate) WHERE IsDeleted = 1;

-- RefreshTokens: cleanup expired
CREATE INDEX IX_RefreshTokens_ExpiresAt ON dbo.RefreshTokens(ExpiresAt) WHERE RevokedAt IS NULL;

-- Devices: per user
CREATE INDEX IX_Devices_UserId ON dbo.Devices(UserId);
CREATE INDEX IX_Devices_LastSync ON dbo.Devices(LastSyncAt);

-- KeyCountsDaily: range queries by date + user
CREATE INDEX IX_KeyCountsDaily_UserDate ON dbo.KeyCountsDaily(UserId, EventDate) INCLUDE (PressCount);
CREATE INDEX IX_KeyCountsDaily_DeviceDate ON dbo.KeyCountsDaily(DeviceId, EventDate);

-- KeyCountsHourly: for heatmap and real-time trends
CREATE INDEX IX_KeyCountsHourly_UserHour ON dbo.KeyCountsHourly(UserId, EventHour) INCLUDE (PressCount);
CREATE INDEX IX_KeyCountsHourly_DeviceHour ON dbo.KeyCountsHourly(DeviceId, EventHour);

-- SyncLogs: per device status
CREATE INDEX IX_SyncLogs_DeviceId ON dbo.SyncLogs(DeviceId, StartedAt);

-- AuditLogs: search by user, action, date
CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Action ON dbo.AuditLogs(Action);
CREATE INDEX IX_AuditLogs_CreatedAt ON dbo.AuditLogs(CreatedAt);
GO
-- ============================================================
-- 3. TRIGGERS
-- ============================================================

-- 3.1 Trigger to automatically update Users.UpdatedAt on any change
CREATE TRIGGER trg_Users_UpdateTimestamp
ON dbo.Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Users u
    INNER JOIN inserted i ON u.UserId = i.UserId;
END;
GO

-- 3.2 Trigger to log admin actions when a user is suspended/reactivated
CREATE TRIGGER trg_Users_AuditSuspension
ON dbo.Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditLogs (UserId, ActorType, Action, TargetType, TargetId, Details, CreatedAt)
    SELECT 
        i.UserId,
        'Admin',
        CASE 
            WHEN i.IsSuspended = 1 AND d.IsSuspended = 0 THEN 'USER_SUSPEND'
            WHEN i.IsSuspended = 0 AND d.IsSuspended = 1 THEN 'USER_REACTIVATE'
            ELSE NULL
        END,
        'User',
        i.Username,
        CONCAT('Suspension reason: ', i.SuspensionReason),
        SYSUTCDATETIME()
    FROM inserted i
    INNER JOIN deleted d ON i.UserId = d.UserId
    WHERE (i.IsSuspended != d.IsSuspended) AND i.IsSuspended IS NOT NULL;
END;
GO

-- 3.3 Trigger to prevent negative press counts in KeyCountsDaily (data integrity)
CREATE TRIGGER trg_KeyCountsDaily_CheckNonNegative
ON dbo.KeyCountsDaily
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted WHERE PressCount < 0)
    BEGIN
        RAISERROR('PressCount cannot be negative.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- ============================================================
-- 4. VIEWS (common aggregations for dashboard)
-- ============================================================

-- 4.1 Lifetime total keys per user (all devices)
CREATE VIEW dbo.vw_UserTotalKeys
AS
SELECT 
    UserId,
    SUM(PressCount) AS TotalKeyPressesLifetime
FROM dbo.KeyCountsDaily
GROUP BY UserId;
GO

-- 4.2 Daily summary for a user (for trend lines)
CREATE VIEW dbo.vw_UserDailySummary
AS
SELECT 
    UserId,
    EventDate,
    SUM(PressCount) AS DailyTotal
FROM dbo.KeyCountsDaily
GROUP BY UserId, EventDate;
GO

-- 4.3 Top 10 most used keys for a user (overall)
CREATE VIEW dbo.vw_UserTopKeys
AS
SELECT 
    UserId,
    KeyCode,
    KeyDisplayName,
    SUM(PressCount) AS TotalPresses
FROM dbo.KeyCountsDaily
GROUP BY UserId, KeyCode, KeyDisplayName
GO
-- (Usage: SELECT * FROM vw_UserTopKeys WHERE UserId = @uid ORDER BY TotalPresses DESC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY)

-- 4.4 Device stats with last sync and total keys
CREATE VIEW dbo.vw_DeviceStats
AS
SELECT 
    d.DeviceId,
    d.UserId,
    d.DeviceName,
    d.OperatingSystem,
    d.ClientVersion,
    d.LastSyncAt,
    d.IsPrimary,
    ISNULL(SUM(kcd.PressCount), 0) AS TotalKeysRecorded
FROM dbo.Devices d
LEFT JOIN dbo.KeyCountsDaily kcd ON d.DeviceId = kcd.DeviceId
GROUP BY d.DeviceId, d.UserId, d.DeviceName, d.OperatingSystem, d.ClientVersion, d.LastSyncAt, d.IsPrimary;
GO

-- 4.5 Hourly activity heatmap data (for a given user + date range)
CREATE VIEW dbo.vw_HourlyActivity
AS
SELECT 
    UserId,
    DeviceId,
    DATEPART(HOUR, EventHour) AS HourOfDay,
    SUM(PressCount) AS TotalPresses
FROM dbo.KeyCountsHourly
GROUP BY UserId, DeviceId, DATEPART(HOUR, EventHour);

-- ============================================================
-- 5. STORED PROCEDURES (Optional – examples for common operations)
-- ============================================================

-- 5.1 Synchronization: upsert daily key counts (FR-5.4, additive merge)
GO
CREATE OR ALTER PROCEDURE dbo.sp_UpsertKeyCountsDaily
    @UserId INT,
    @DeviceId INT,
    @KeyCode NVARCHAR(50),
    @KeyDisplayName NVARCHAR(50),
    @EventDate DATE,
    @IncrementCount INT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.KeyCountsDaily AS target
    USING (SELECT @UserId AS UserId, @DeviceId AS DeviceId, @KeyCode AS KeyCode, @EventDate AS EventDate) AS source
    ON (target.UserId = source.UserId AND target.DeviceId = source.DeviceId 
        AND target.KeyCode = source.KeyCode AND target.EventDate = source.EventDate)
    WHEN MATCHED THEN
        UPDATE SET PressCount = target.PressCount + @IncrementCount,
                   ModifiedAt = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (UserId, DeviceId, KeyCode, KeyDisplayName, EventDate, PressCount, ModifiedAt)
        VALUES (@UserId, @DeviceId, @KeyCode, @KeyDisplayName, @EventDate, @IncrementCount, SYSUTCDATETIME());
END;
GO

-- 5.2 Soft delete user account (GDPR - FR-14.4)
USE KeyMapperDb;
GO
CREATE OR ALTER PROCEDURE dbo.sp_RequestAccountDeletion
    @UserId INT,
    @PasswordHash NVARCHAR(255)   -- should be verified by application layer
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET IsDeleted = 1,
        DeletionRequestDate = SYSUTCDATETIME(),
        PermanentDeletionDate = DATEADD(DAY, 30, SYSUTCDATETIME())
    WHERE UserId = @UserId;
    
    -- Revoke all refresh tokens
    UPDATE dbo.RefreshTokens SET RevokedAt = SYSUTCDATETIME() WHERE UserId = @UserId;
    
    -- Log the deletion request
    INSERT INTO dbo.AuditLogs (UserId, ActorType, Action, TargetType, TargetId, CreatedAt)
    VALUES (@UserId, 'User', 'ACCOUNT_DELETION_REQUEST', 'User', CAST(@UserId AS NVARCHAR), SYSUTCDATETIME());
END;
GO

-- ============================================================
-- 6. MAINTENANCE: Cleanup job for expired data (to be scheduled)
-- ============================================================
-- Delete expired refresh tokens daily
-- DELETE FROM dbo.RefreshTokens WHERE ExpiresAt < SYSUTCDATETIME() OR RevokedAt IS NOT NULL;

-- Permanently delete users whose PermanentDeletionDate has passed (FR-14.6)
-- DELETE FROM dbo.Users WHERE IsDeleted = 1 AND PermanentDeletionDate < SYSUTCDATETIME();
-- (Cascade constraints will remove related records automatically)

-- ============================================================
-- End of schema
-- ============================================================
