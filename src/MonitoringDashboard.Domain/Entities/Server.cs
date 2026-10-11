using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Entities;

public sealed class Server
{
    public int ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public ServerStatus Status { get; set; } = ServerStatus.Up;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    public ICollection<Disk> Disks { get; set; } = new List<Disk>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}