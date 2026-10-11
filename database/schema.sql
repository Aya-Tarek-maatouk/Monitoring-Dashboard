IF DB_ID(N'MonitoringDashboard') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [MonitoringDashboard]');
END;
GO

USE [MonitoringDashboard];
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL,
        Description NVARCHAR(250) NULL,
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions
    (
        PermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NULL,
        CONSTRAINT UQ_Permissions_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions
    (
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(PermissionId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        RoleId INT NOT NULL,
        RefreshToken NVARCHAR(500) NULL,
        RefreshTokenExpiry DATETIME2(3) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Servers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Servers
    (
        ServerId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Servers PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        IPAddress NVARCHAR(50) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Servers_Status DEFAULT N'Up',
        Description NVARCHAR(250) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Servers_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Servers_Status CHECK (Status IN (N'Unknown', N'Up', N'Down'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Metrics', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Metrics
    (
        MetricId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Metrics PRIMARY KEY,
        ServerId INT NOT NULL,
        CpuUsage FLOAT NOT NULL,
        MemoryUsage FLOAT NOT NULL,
        DiskUsage FLOAT NOT NULL,
        ResponseTime FLOAT NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        Timestamp DATETIME2(3) NOT NULL CONSTRAINT DF_Metrics_Timestamp DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Metrics_CpuUsage CHECK (CpuUsage BETWEEN 0 AND 100),
        CONSTRAINT CK_Metrics_MemoryUsage CHECK (MemoryUsage BETWEEN 0 AND 100),
        CONSTRAINT CK_Metrics_DiskUsage CHECK (DiskUsage BETWEEN 0 AND 100),
        CONSTRAINT CK_Metrics_ResponseTime CHECK (ResponseTime >= 0),
        CONSTRAINT CK_Metrics_Status CHECK (Status IN (N'Unknown', N'Up', N'Down')),
        CONSTRAINT FK_Metrics_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(ServerId)
    );
    CREATE INDEX IX_Metrics_ServerId_Timestamp ON dbo.Metrics(ServerId, Timestamp);
END;
GO

IF OBJECT_ID(N'dbo.Disks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Disks
    (
        DiskId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Disks PRIMARY KEY,
        ServerId INT NOT NULL,
        DriveLetter NVARCHAR(5) NOT NULL,
        FreeSpaceMB BIGINT NOT NULL,
        TotalSpaceMB BIGINT NOT NULL,
        UsedPercentage FLOAT NOT NULL,
        Timestamp DATETIME2(3) NOT NULL CONSTRAINT DF_Disks_Timestamp DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Disks_Space CHECK (FreeSpaceMB >= 0 AND TotalSpaceMB >= FreeSpaceMB),
        CONSTRAINT CK_Disks_UsedPercentage CHECK (UsedPercentage BETWEEN 0 AND 100),
        CONSTRAINT FK_Disks_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(ServerId)
    );
    CREATE INDEX IX_Disks_ServerId_Timestamp ON dbo.Disks(ServerId, Timestamp);
END;
GO

IF OBJECT_ID(N'dbo.Alerts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Alerts
    (
        AlertId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Alerts PRIMARY KEY,
        ServerId INT NOT NULL,
        MetricType NVARCHAR(50) NOT NULL,
        MetricValue FLOAT NOT NULL,
        Threshold FLOAT NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Alerts_Status DEFAULT N'Triggered',
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Alerts_CreatedAt DEFAULT SYSUTCDATETIME(),
        ResolvedAt DATETIME2(3) NULL,
        CONSTRAINT CK_Alerts_Status CHECK (Status IN (N'Triggered', N'Acknowledged', N'Resolved')),
        CONSTRAINT FK_Alerts_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(ServerId)
    );
    CREATE INDEX IX_Alerts_ServerId_CreatedAt ON dbo.Alerts(ServerId, CreatedAt);
END;
GO

IF OBJECT_ID(N'dbo.Reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reports
    (
        ReportId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reports PRIMARY KEY,
        ServerId INT NOT NULL,
        StartTime DATETIME2(3) NOT NULL,
        EndTime DATETIME2(3) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Reports_Status DEFAULT N'Pending',
        FilePath NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Reports_CreatedAt DEFAULT SYSUTCDATETIME(),
        CompletedAt DATETIME2(3) NULL,
        CONSTRAINT CK_Reports_TimeRange CHECK (StartTime < EndTime),
        CONSTRAINT CK_Reports_Status CHECK (Status IN (N'Pending', N'Processing', N'Completed', N'Failed')),
        CONSTRAINT FK_Reports_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(ServerId)
    );
    CREATE INDEX IX_Reports_ServerId_TimeRange ON dbo.Reports(ServerId, StartTime, EndTime);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'User')
    INSERT dbo.Roles (Name, Description) VALUES (N'User', N'Standard dashboard user');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
    INSERT dbo.Roles (Name, Description) VALUES (N'Admin', N'Dashboard administrator');
GO

IF COL_LENGTH(N'dbo.Users', N'RoleId') IS NULL
    ALTER TABLE dbo.Users ADD RoleId INT NULL;
GO

DECLARE @DefaultRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE Name = N'User');

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL
BEGIN
    ;WITH RankedUserRoles AS
    (
        SELECT userRoles.UserId,
               userRoles.RoleId,
               ROW_NUMBER() OVER
               (
                   PARTITION BY userRoles.UserId
                   ORDER BY CASE WHEN assignedRole.Name = N'Admin' THEN 0 ELSE 1 END,
                            userRoles.RoleId
               ) AS RoleRank
        FROM dbo.UserRoles AS userRoles
        JOIN dbo.Roles AS assignedRole ON assignedRole.RoleId = userRoles.RoleId
    )
    UPDATE users
    SET RoleId = COALESCE(assignedRole.RoleId, @DefaultRoleId)
    FROM dbo.Users AS users
    LEFT JOIN RankedUserRoles AS assignedRole
        ON assignedRole.UserId = users.UserId AND assignedRole.RoleRank = 1
    WHERE users.RoleId IS NULL;
END;

UPDATE dbo.Users SET RoleId = @DefaultRoleId WHERE RoleId IS NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'RoleId' AND is_nullable = 1)
    ALTER TABLE dbo.Users ALTER COLUMN RoleId INT NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_Roles' AND parent_object_id = OBJECT_ID(N'dbo.Users'))
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId);

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL
    DROP TABLE dbo.UserRoles;
GO

INSERT dbo.Permissions (Name, Description)
SELECT seeded.Name, seeded.Description
FROM (VALUES
    (N'servers.read', N'View monitored servers'),
    (N'servers.manage', N'Create, update, and delete monitored servers'),
    (N'metrics.read', N'View server metric history'),
    (N'metrics.write', N'Record server metric samples'),
    (N'alerts.read', N'View alert history'),
    (N'reports.read', N'View and download reports'),
    (N'reports.create', N'Queue performance reports'),
    (N'users.manage', N'View and manage user accounts'),
    (N'jobs.read', N'View background job history'),
    (N'jobs.retry', N'Retry failed background jobs')
) AS seeded(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions AS existing WHERE existing.Name = seeded.Name);
GO

INSERT dbo.RolePermissions (RoleId, PermissionId)
SELECT roles.RoleId, permissions.PermissionId
FROM dbo.Roles AS roles
CROSS JOIN dbo.Permissions AS permissions
WHERE roles.Name = N'Admin'
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.RolePermissions AS grants
      WHERE grants.RoleId = roles.RoleId AND grants.PermissionId = permissions.PermissionId
  );

INSERT dbo.RolePermissions (RoleId, PermissionId)
SELECT roles.RoleId, permissions.PermissionId
FROM dbo.Roles AS roles
JOIN dbo.Permissions AS permissions ON permissions.Name IN
    (N'servers.read', N'metrics.read', N'metrics.write', N'alerts.read', N'reports.read', N'reports.create', N'jobs.read')
WHERE roles.Name = N'User'
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.RolePermissions AS grants
      WHERE grants.RoleId = roles.RoleId AND grants.PermissionId = permissions.PermissionId
  );
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Servers WHERE Name = N'demo-server-01')
    INSERT dbo.Servers (Name, IPAddress, Description, Status)
    VALUES (N'demo-server-01', N'127.0.0.1', N'Seeded demonstration server', N'Up');
GO