using MonitoringDashboard.Application.Features.Roles.DTOs;

namespace MonitoringDashboard.Application.Features.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleResponse> GetByIdAsync(int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken);
}