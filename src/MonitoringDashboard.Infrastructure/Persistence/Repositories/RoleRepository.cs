using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Roles.AsNoTracking().Include(role => role.RolePermissions).ThenInclude(grant => grant.Permission)
            .OrderBy(role => role.Name).ToListAsync(cancellationToken);

    public Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken) =>
        context.Roles.AsNoTracking().Include(role => role.RolePermissions).ThenInclude(grant => grant.Permission)
            .SingleOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);

    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken) =>
        await context.Permissions.OrderBy(permission => permission.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        context.Roles.AnyAsync(role => role.Name.ToUpper() == normalizedName, cancellationToken);

    public void Add(Role role) => context.Roles.Add(role);
}