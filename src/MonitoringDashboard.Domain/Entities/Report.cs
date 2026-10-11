using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Entities;

public sealed class Report
{
    public int ReportId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}