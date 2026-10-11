using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Entities;

public sealed class Metric
{
    public int MetricId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double ResponseTime { get; set; }
    public ServerStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
}