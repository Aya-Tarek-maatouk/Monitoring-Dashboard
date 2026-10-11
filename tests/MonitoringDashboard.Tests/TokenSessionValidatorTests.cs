using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;
using MonitoringDashboard.WebAPI.Services;

namespace MonitoringDashboard.Tests;

public sealed class TokenSessionValidatorTests
{
    [Theory]
    [InlineData("User", "servers.read", true)]
    [InlineData("Admin", "servers.read", false)]
    [InlineData("User", "servers.manage", false)]
    [InlineData("User", "", false)]
    public async Task IsCurrentAsync_RejectsStaleRoleOrPermissionClaims(string role, string permission, bool expected)
    {
        await using var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var userRole = new Role { Name = "User" };
        userRole.RolePermissions.Add(new RolePermission
        {
            Role = userRole,
            Permission = new Permission { Name = "servers.read" }
        });
        var user = new User { UserName = "test-user", Email = "test@example.test", Role = userRole };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Role, role)
        };
        if (permission.Length > 0) claims.Add(new Claim("permission", permission));
        var validator = new TokenSessionValidator(new UserRepository(context));

        Assert.Equal(expected, await validator.IsCurrentAsync(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "test")), CancellationToken.None));
    }

    [Theory]
    [InlineData("999")]
    [InlineData("invalid")]
    public async Task IsCurrentAsync_RejectsMissingUsersAndInvalidIdentifiers(string userId)
    {
        await using var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, "User")], "test"));
        var validator = new TokenSessionValidator(new UserRepository(context));

        Assert.False(await validator.IsCurrentAsync(principal, CancellationToken.None));
    }
}