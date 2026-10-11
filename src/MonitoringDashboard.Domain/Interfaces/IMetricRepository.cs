using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IMetricRepository
{
    Task<(IReadOnlyList<Metric> Items, int TotalCount)> GetPageAsync(
        int serverId,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Metric>> GetRangeAsync(
        int serverId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken);

    Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken);
    void Add(Metric metric);
}