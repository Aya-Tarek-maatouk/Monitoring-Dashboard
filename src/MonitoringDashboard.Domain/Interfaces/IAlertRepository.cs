using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IAlertRepository
{
    Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetPageAsync(
        int? serverId,
        AlertStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Alert>> GetActiveForServerAsync(int serverId, CancellationToken cancellationToken);
    Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken);
    void AddRange(IEnumerable<Alert> alerts);
}