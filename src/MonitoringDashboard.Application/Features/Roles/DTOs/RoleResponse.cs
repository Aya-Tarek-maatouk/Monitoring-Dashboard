namespace MonitoringDashboard.Application.Features.Roles.DTOs;

public sealed record RoleResponse(int RoleId, string Name, string? Description, IReadOnlyList<PermissionResponse> Permissions);