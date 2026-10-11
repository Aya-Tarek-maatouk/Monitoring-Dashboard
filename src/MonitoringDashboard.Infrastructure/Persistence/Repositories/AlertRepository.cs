using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository(ApplicationDbContext context) : IAlertRepository
{
    public async Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetPageAsync(
        int? serverId,
        AlertStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Alerts.AsNoTracking();
        if (serverId.HasValue)
        {
            query = query.Where(alert => alert.ServerId == serverId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(alert => alert.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(alert => alert.CreatedAt)
            .ThenByDescending(alert => alert.AlertId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Alert>> GetActiveForServerAsync(int serverId, CancellationToken cancellationToken) =>
        await context.Alerts
            .Where(alert => alert.ServerId == serverId && alert.Status != AlertStatus.Resolved)
            .ToListAsync(cancellationToken);

    public Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken) =>
        context.Alerts.AnyAsync(alert => alert.ServerId == serverId, cancellationToken);

    public void AddRange(IEnumerable<Alert> alerts) => context.Alerts.AddRange(alerts);
}