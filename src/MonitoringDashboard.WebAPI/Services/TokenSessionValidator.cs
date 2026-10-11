using System.Security.Claims;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.WebAPI.Services;

public sealed class TokenSessionValidator(IUserRepository users)
{
    public async Task<bool> IsCurrentAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return false;
        }

        var user = await users.GetWithRoleAndPermissionsByIdAsync(userId, cancellationToken);
        var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
        if (user is null || roles.Length != 1 || !string.Equals(roles[0], user.Role.Name, StringComparison.Ordinal))
        {
            return false;
        }

        var currentPermissions = user.Role.RolePermissions
            .Select(rolePermission => rolePermission.Permission.Name)
            .ToHashSet(StringComparer.Ordinal);
        return currentPermissions.SetEquals(principal.FindAll("permission").Select(claim => claim.Value));
    }
}