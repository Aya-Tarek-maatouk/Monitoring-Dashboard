using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Metrics.DTOs;

public sealed record RecordMetricRequest
{
    public required double CpuUsage { get; init; }

    public required double MemoryUsage { get; init; }

    public required double DiskUsage { get; init; }

    public required double ResponseTime { get; init; }

    public required ServerStatus Status { get; init; }
}