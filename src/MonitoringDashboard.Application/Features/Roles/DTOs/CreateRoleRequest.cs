namespace MonitoringDashboard.Application.Features.Roles.DTOs;

public sealed record CreateRoleRequest
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public int[] PermissionIds { get; init; } = [];
}