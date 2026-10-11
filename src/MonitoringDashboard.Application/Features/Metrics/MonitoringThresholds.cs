namespace MonitoringDashboard.Application.Features.Metrics;

public sealed class MonitoringThresholds
{
    public double CpuUsage { get; init; } = 80;
    public double MemoryUsage { get; init; } = 90;
    public double DiskUsage { get; init; } = 90;
}