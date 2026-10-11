using MonitoringDashboard.Application.Features.Metrics.DTOs;

namespace MonitoringDashboard.Application.Features.Metrics;

public interface IMetricService
{
    Task<MetricResponse> RecordAsync(
        int serverId,
        RecordMetricRequest request,
        CancellationToken cancellationToken);

    Task<MetricHistoryResponse> GetHistoryAsync(
        int serverId,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);
}