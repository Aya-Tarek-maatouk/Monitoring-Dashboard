using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken);
    Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken cancellationToken);
    void Add(Role role);
}