using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class ServerRepository(ApplicationDbContext context) : IServerRepository
{
    public async Task<(IReadOnlyList<Server> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Servers.AsNoTracking().OrderBy(server => server.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Server?> GetByIdAsync(int serverId, CancellationToken cancellationToken) =>
        context.Servers.SingleOrDefaultAsync(server => server.ServerId == serverId, cancellationToken);

    public Task<Server?> GetReadOnlyByIdAsync(int serverId, CancellationToken cancellationToken) =>
        context.Servers.AsNoTracking().SingleOrDefaultAsync(server => server.ServerId == serverId, cancellationToken);

    public async Task<IReadOnlyList<int>> GetServerIdsAsync(CancellationToken cancellationToken) =>
        await context.Servers.AsNoTracking()
            .OrderBy(server => server.ServerId)
            .Select(server => server.ServerId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int serverId, CancellationToken cancellationToken) =>
        context.Servers.AnyAsync(server => server.ServerId == serverId, cancellationToken);

    public async Task<bool> HasMonitoringHistoryAsync(int serverId, CancellationToken cancellationToken) =>
        await context.Metrics.AnyAsync(metric => metric.ServerId == serverId, cancellationToken)
        || await context.Disks.AnyAsync(disk => disk.ServerId == serverId, cancellationToken)
        || await context.Alerts.AnyAsync(alert => alert.ServerId == serverId, cancellationToken)
        || await context.Reports.AnyAsync(report => report.ServerId == serverId, cancellationToken);

    public void Add(Server server) => context.Servers.Add(server);

    public void Remove(Server server) => context.Servers.Remove(server);
}