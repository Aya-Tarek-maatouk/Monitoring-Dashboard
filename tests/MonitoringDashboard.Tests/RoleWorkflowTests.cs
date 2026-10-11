using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Features.Roles;
using MonitoringDashboard.Application.Features.Roles.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;

namespace MonitoringDashboard.Tests;

public sealed class RoleWorkflowTests
{
    [Fact]
    public async Task CreateAsync_PersistsCustomRoleWithSelectedPermissions()
    {
        await using var context = CreateContext();
        var permission = new Permission { Name = "servers.read" };
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();
        var service = new RoleService(new RoleRepository(context), context);
        var role = await service.CreateAsync(new CreateRoleRequest
        {
            Name = " Observer ",
            PermissionIds = [permission.PermissionId, permission.PermissionId]
        }, CancellationToken.None);
        Assert.Equal("Observer", role.Name);
        Assert.Single(role.Permissions);
        Assert.Equal(role.RoleId, (await service.GetAllAsync(CancellationToken.None)).Single().RoleId);
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateRoleRequest
        {
            Name = "observer"
        }, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidNamesAndUnknownPermissions()
    {
        await using var context = CreateContext();
        var service = new RoleService(new RoleRepository(context), context);
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(
            new CreateRoleRequest { Name = " " }, CancellationToken.None));
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(
            new CreateRoleRequest { Name = "Observer", PermissionIds = [999] }, CancellationToken.None));
        Assert.Empty(await context.Roles.ToListAsync());
    }

    private static ApplicationDbContext CreateContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}