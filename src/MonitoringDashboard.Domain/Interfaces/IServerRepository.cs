using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IServerRepository
{
    Task<(IReadOnlyList<Server> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Server?> GetByIdAsync(int serverId, CancellationToken cancellationToken);
    Task<Server?> GetReadOnlyByIdAsync(int serverId, CancellationToken cancellationToken);
    Task<IReadOnlyList<int>> GetServerIdsAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int serverId, CancellationToken cancellationToken);
    Task<bool> HasMonitoringHistoryAsync(int serverId, CancellationToken cancellationToken);
    void Add(Server server);
    void Remove(Server server);
}