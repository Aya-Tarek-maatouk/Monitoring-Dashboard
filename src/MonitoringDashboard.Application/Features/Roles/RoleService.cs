using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Roles.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Roles;

public sealed class RoleService(IRoleRepository roles, IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        (await roles.GetAllAsync(cancellationToken)).Select(ToResponse).ToArray();

    public async Task<RoleResponse> GetByIdAsync(int roleId, CancellationToken cancellationToken) =>
        ToResponse(await roles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException(ValidationMessages.RoleNotFound));

    public async Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken) =>
        (await roles.GetPermissionsAsync(cancellationToken))
            .Select(permission => new PermissionResponse(permission.PermissionId, permission.Name, permission.Description)).ToArray();

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 50) throw new BadRequestException(ValidationMessages.RoleNameInvalid);
        if (request.Description?.Length > 250) throw new BadRequestException(ValidationMessages.RoleDescriptionInvalid);
        if (request.PermissionIds is null || request.PermissionIds.Any(permissionId => permissionId <= 0))
            throw new BadRequestException(ValidationMessages.PermissionIdsInvalid);

        return await unitOfWork.ExecuteSerializableAsync(async token =>
        {
            if (await roles.ExistsByNameAsync(name.ToUpperInvariant(), token))
                throw new ConflictException(ValidationMessages.RoleAlreadyExists);
            var permissionIds = request.PermissionIds.ToHashSet();
            var permissions = (await roles.GetPermissionsAsync(token))
                .Where(permission => permissionIds.Contains(permission.PermissionId)).ToArray();
            if (permissions.Length != permissionIds.Count) throw new BadRequestException(ValidationMessages.PermissionIdsInvalid);
            var role = new Role { Name = name, Description = request.Description?.Trim() };
            foreach (var permission in permissions)
            {
                role.RolePermissions.Add(new RolePermission { Role = role, Permission = permission });
            }
            roles.Add(role);
            await unitOfWork.SaveChangesAsync(token);
            return ToResponse(role);
        }, cancellationToken);
    }

    private static RoleResponse ToResponse(Role role) => new(role.RoleId, role.Name, role.Description,
        role.RolePermissions.Select(grant => new PermissionResponse(grant.Permission.PermissionId,
            grant.Permission.Name, grant.Permission.Description)).OrderBy(permission => permission.Name).ToArray());
}