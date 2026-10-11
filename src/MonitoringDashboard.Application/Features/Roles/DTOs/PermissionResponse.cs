namespace MonitoringDashboard.Application.Features.Roles.DTOs;

public sealed record PermissionResponse(int PermissionId, string Name, string? Description);