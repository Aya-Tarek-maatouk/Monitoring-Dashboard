using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class MetricRepository(ApplicationDbContext context) : IMetricRepository
{
    public async Task<(IReadOnlyList<Metric> Items, int TotalCount)> GetPageAsync(
        int serverId,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var query = context.Metrics.AsNoTracking().Where(metric => metric.ServerId == serverId);
        if (from.HasValue)
        {
            query = query.Where(metric => metric.Timestamp >= from.Value.UtcDateTime);
        }

        if (to.HasValue)
        {
            query = query.Where(metric => metric.Timestamp <= to.Value.UtcDateTime);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(metric => metric.Timestamp)
            .ThenByDescending(metric => metric.MetricId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Metric>> GetRangeAsync(
        int serverId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken) =>
        await context.Metrics.AsNoTracking()
            .Where(metric => metric.ServerId == serverId
                && metric.Timestamp >= startTime
                && metric.Timestamp <= endTime)
            .OrderBy(metric => metric.Timestamp)
            .ThenBy(metric => metric.MetricId)
            .ToListAsync(cancellationToken);

    public Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken) =>
        context.Metrics.AnyAsync(metric => metric.ServerId == serverId, cancellationToken);

    public void Add(Metric metric) => context.Metrics.Add(metric);
}