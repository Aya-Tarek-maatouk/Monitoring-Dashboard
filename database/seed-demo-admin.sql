-- Username: demo.admin
-- Password: DemoAdmin!2026
USE [MonitoringDashboard];
GO

DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE Name = N'Admin');

IF @AdminRoleId IS NULL
    THROW 50001, 'Run schema.sql before seeding the demo administrator.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = N'demo.admin' OR Email = N'demo.admin@example.test')
BEGIN
    INSERT dbo.Users (UserName, Email, PasswordHash, RoleId)
    VALUES (
        N'demo.admin',
        N'demo.admin@example.test',
        N'AQAAAAIAAYagAAAAEIvV2ogrzfdSNuA/xkQhMxeMTtwKNA3CMBDFpjc5HH9J/BJYk86/SFfDN6Oik7xhwA==',
        @AdminRoleId
    );
END;
GO