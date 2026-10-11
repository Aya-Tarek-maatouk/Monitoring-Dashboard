using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Domain.Interfaces;

public interface IReportRepository
{
    Task<(IReadOnlyList<Report> Items, int TotalCount)> GetPageAsync(
        int? serverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Report?> GetByIdAsync(int reportId, CancellationToken cancellationToken);
    Task<Report?> GetReadOnlyByIdAsync(int reportId, CancellationToken cancellationToken);
    Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken);
    void Add(Report report);
}