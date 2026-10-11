using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Users;
using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;

namespace MonitoringDashboard.Tests;

public sealed class UserAuthenticationTests
{
    [Fact]
    public async Task ChangeRoleAsync_AcceptsCustomRolesAndRejectsMissingRoleIds()
    {
        await using var context = CreateContext();
        await SeedUserRoleAsync(context);
        var customRole = new Role { Name = "Auditor" };
        context.Roles.Add(customRole);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var user = await service.RegisterAsync(CreateRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ChangeRoleAsync(user.UserId,
            new ChangeUserRoleRequest { RoleId = 999 }, CancellationToken.None));
        var assigned = await service.ChangeRoleAsync(user.UserId,
            new ChangeUserRoleRequest { RoleId = customRole.RoleId }, CancellationToken.None);
        Assert.Equal(customRole.RoleId, assigned.RoleId);
        Assert.Equal("Auditor", assigned.Role);
    }

    [Fact]
    public async Task ChangeRoleAsync_PromotesUsersAndProtectsLastAdmin()
    {
        await using var context = CreateContext();
        await SeedUserRoleAsync(context);
        var admin = new Role { Name = "Admin" };
        admin.RolePermissions.Add(new RolePermission
        {
            Role = admin,
            Permission = new Permission { Name = "users.manage" }
        });
        context.Roles.Add(admin);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var first = await service.RegisterAsync(CreateRequest(), CancellationToken.None);
        var standardRoleId = (await context.Roles.SingleAsync(role => role.Name == "User")).RoleId;
        var second = await service.RegisterAsync(CreateRequest() with
        {
            UserName = "second-user",
            Email = "second@example.test"
        }, CancellationToken.None);

        var promoted = await service.ChangeRoleAsync(first.UserId,
            new ChangeUserRoleRequest { RoleId = admin.RoleId }, CancellationToken.None);
        Assert.Equal("Admin", promoted.Role);
        Assert.Contains("users.manage", promoted.Permissions);
        admin.Name = "Operations";
        await context.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => service.ChangeRoleAsync(first.UserId,
            new ChangeUserRoleRequest { RoleId = standardRoleId }, CancellationToken.None));
        await service.ChangeRoleAsync(second.UserId, new ChangeUserRoleRequest { RoleId = admin.RoleId }, CancellationToken.None);
        var demoted = await service.ChangeRoleAsync(first.UserId,
            new ChangeUserRoleRequest { RoleId = standardRoleId }, CancellationToken.None);
        Assert.Equal("User", demoted.Role);
        Assert.Single(await context.Users.Where(user => user.RoleId == admin.RoleId).ToListAsync());
    }

    [Fact]
    public async Task ChangeRoleAsync_RejectsInvalidRolesAndMissingUsers()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await Assert.ThrowsAsync<BadRequestException>(() => service.ChangeRoleAsync(1,
            new ChangeUserRoleRequest { RoleId = 0 }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => service.ChangeRoleAsync(999,
            new ChangeUserRoleRequest { RoleId = 1 }, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_WhenUserRoleIsMissing_FailsWithoutCreatingUserOrRole()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(CreateRequest(), CancellationToken.None));

        Assert.Equal("The required User role has not been seeded.", exception.Message);
        Assert.Empty(await context.Users.ToListAsync());
        Assert.Empty(await context.Roles.ToListAsync());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task RegisterAsync_AssignsOneRoleAndReturnsItsPermissions()
    {
        await using var context = CreateContext();
        await SeedUserRoleAsync(context);
        var service = CreateService(context);

        var response = await service.RegisterAsync(CreateRequest(), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("dashboard-user", response.UserName);
        Assert.Equal("User", response.Role);
        Assert.Contains("servers.read", response.Permissions);
        var storedUser = await context.Users.SingleAsync();
        Assert.NotEqual("correct horse battery staple", storedUser.PasswordHash);
        Assert.Equal("person@example.test", storedUser.Email);
        Assert.Equal("User", storedUser.Role.Name);
        Assert.Equal("User", (await context.Roles.SingleAsync()).Name);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateUsernameOrEmail()
    {
        await using var context = CreateContext();
        await SeedUserRoleAsync(context);
        var service = CreateService(context);

        Assert.NotNull(await service.RegisterAsync(CreateRequest(), CancellationToken.None));
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(
            CreateRequest() with { Email = "another@example.test" }, CancellationToken.None));
        Assert.Equal("A user with that username or email already exists.", exception.Message);
        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(
            CreateRequest() with { UserName = "another-user" }, CancellationToken.None));
        Assert.Single(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenOnlyForValidCredentials()
    {
        await using var context = CreateContext();
        await SeedUserRoleAsync(context);
        var service = CreateService(context);
        await service.RegisterAsync(CreateRequest(), CancellationToken.None);

        var login = await service.LoginAsync(new LoginRequest
        {
            UserNameOrEmail = "person@example.test",
            Password = "correct horse battery staple"
        }, CancellationToken.None);
        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequest
        {
            UserNameOrEmail = "person@example.test",
            Password = "incorrect password"
        }, CancellationToken.None));

        Assert.NotNull(login);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedUserRoleAsync(ApplicationDbContext context)
    {
        var permission = new Permission { Name = "servers.read", Description = "View monitored servers" };
        var role = new Role { Name = "User", Description = "Standard dashboard user" };
        role.RolePermissions.Add(new RolePermission { Role = role, Permission = permission });
        context.Roles.Add(role);
        await context.SaveChangesAsync();
    }

    private static UserService CreateService(ApplicationDbContext context) => new(
        new UserRepository(context),
        context,
        new TestPasswordService(),
        new TestAccessTokenService(),
        TimeProvider.System);

    private static RegisterUserRequest CreateRequest() => new()
    {
        UserName = "dashboard-user",
        Email = "person@example.test",
        Password = "correct horse battery staple"
    };

    private sealed class TestPasswordService : IUserPasswordService
    {
        public string Hash(User user, string password) => $"hashed:{password}";

        public bool Verify(User user, string passwordHash, string password) => passwordHash == $"hashed:{password}";
    }

    private sealed class TestAccessTokenService : IAccessTokenService
    {
        public AuthenticationResponse CreateToken(User user, string role, IReadOnlyCollection<string> permissions) => new(
            user.UserId,
            user.UserName,
            role,
            permissions.ToArray(),
            "test-token",
            DateTimeOffset.UtcNow.AddMinutes(60));
    }
}