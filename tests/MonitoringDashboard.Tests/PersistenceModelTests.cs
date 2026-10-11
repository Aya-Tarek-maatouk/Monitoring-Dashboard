using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Persistence;

namespace MonitoringDashboard.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void User_HasSingleRequiredRoleForeignKey()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(entity);
        var foreignKey = Assert.Single(entity.GetForeignKeys());
        Assert.Equal(typeof(Role), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(User.RoleId), Assert.Single(foreignKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void RolePermissions_UseCompositePrimaryKey()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(RolePermission));

        Assert.NotNull(entity);
        Assert.Equal(
            new[] { nameof(RolePermission.RoleId), nameof(RolePermission.PermissionId) },
            entity.FindPrimaryKey()!.Properties.Select(property => property.Name));
    }

    [Fact]
    public void ServerHistory_UsesRestrictiveForeignKeys()
    {
        using var context = CreateContext();
        var metric = context.Model.FindEntityType(typeof(Metric));
        var foreignKey = Assert.Single(metric!.GetForeignKeys());

        Assert.Equal(typeof(Server), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelTests;Trusted_Connection=True")
            .Options;

        return new ApplicationDbContext(options);
    }
}