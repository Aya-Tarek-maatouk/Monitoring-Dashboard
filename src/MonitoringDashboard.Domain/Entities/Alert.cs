using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Entities;

public sealed class Alert
{
    public int AlertId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public string MetricType { get; set; } = string.Empty;
    public double MetricValue { get; set; }
    public double Threshold { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Triggered;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}