USE [MonitoringDashboard];
GO

DECLARE @UserName NVARCHAR(50) = N'REPLACE_WITH_REGISTERED_USERNAME';
DECLARE @UserId INT = (SELECT UserId FROM dbo.Users WHERE UserName = @UserName);
DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE Name = N'Admin');

IF @UserId IS NULL
    THROW 50001, 'Register the target user before promoting it.', 1;

IF @AdminRoleId IS NULL
    THROW 50002, 'Run schema.sql to seed the Admin role first.', 1;

UPDATE dbo.Users SET RoleId = @AdminRoleId WHERE UserId = @UserId;
GO