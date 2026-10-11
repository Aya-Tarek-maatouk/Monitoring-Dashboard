using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<bool> ExistsByUserNameOrEmailAsync(
        string userName,
        string email,
        CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.UserName == userName || user.Email == email, cancellationToken);

    public Task<Role?> GetRoleWithPermissionsAsync(string roleName, CancellationToken cancellationToken) =>
        context.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken);

    public Task<User?> GetWithRoleAndPermissionsAsync(
        string identifier,
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        context.Users
            .Include(user => user.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(
                user => user.UserName == identifier || user.Email == normalizedEmail,
                cancellationToken);

    public Task<User?> GetWithRoleAndPermissionsByIdAsync(int userId, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking()
            .Include(user => user.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken) =>
        context.Users.Include(user => user.Role)
            .SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);

    public Task<int> CountByRoleAsync(string roleName, CancellationToken cancellationToken) =>
        context.Users.CountAsync(user => user.Role.Name == roleName, cancellationToken);

    public Task<int> CountByPermissionAsync(string permissionName, CancellationToken cancellationToken) =>
        context.Users.CountAsync(user => user.Role.RolePermissions.Any(
            grant => grant.Permission.Name == permissionName), cancellationToken);

    public Task<Role?> GetRoleWithPermissionsByIdAsync(int roleId, CancellationToken cancellationToken) =>
        context.Roles.Include(role => role.RolePermissions).ThenInclude(grant => grant.Permission)
            .SingleOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageWithRolesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(user => user.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(user => user.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(User user) => context.Users.Add(user);

    public void AddRole(Role role) => context.Roles.Add(role);
}