using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Metrics.DTOs;

public sealed record MetricResponse(
    int MetricId,
    int ServerId,
    double CpuUsage,
    double MemoryUsage,
    double DiskUsage,
    double ResponseTime,
    ServerStatus Status,
    DateTimeOffset Timestamp);