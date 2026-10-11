using Microsoft.EntityFrameworkCore;
using System.Data;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Disk> Disks => Set<Disk>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Report> Reports => Set<Report>();

    public async Task<TResult> ExecuteSerializableAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (!Database.IsRelational())
        {
            return await action(cancellationToken);
        }

        await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var result = await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}