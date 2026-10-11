namespace MonitoringDashboard.Application.Features.Metrics.DTOs;

public sealed record MetricHistoryResponse(
    IReadOnlyList<MetricResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);