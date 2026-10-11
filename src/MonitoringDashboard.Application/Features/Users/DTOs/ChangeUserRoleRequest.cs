namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record ChangeUserRoleRequest
{
    public required int RoleId { get; init; }
}