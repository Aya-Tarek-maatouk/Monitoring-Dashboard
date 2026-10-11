using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email, CancellationToken cancellationToken);
    Task<Role?> GetRoleWithPermissionsAsync(string roleName, CancellationToken cancellationToken);
    Task<User?> GetWithRoleAndPermissionsAsync(string identifier, string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> GetWithRoleAndPermissionsByIdAsync(int userId, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken);
    Task<int> CountByRoleAsync(string roleName, CancellationToken cancellationToken);
    Task<int> CountByPermissionAsync(string permissionName, CancellationToken cancellationToken);
    Task<Role?> GetRoleWithPermissionsByIdAsync(int roleId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageWithRolesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    void Add(User user);
    void AddRole(Role role);
}