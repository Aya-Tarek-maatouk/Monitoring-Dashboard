namespace MonitoringDashboard.Domain.Entities;

public sealed class Disk
{
    public int DiskId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public string DriveLetter { get; set; } = string.Empty;
    public long FreeSpaceMB { get; set; }
    public long TotalSpaceMB { get; set; }
    public double UsedPercentage { get; set; }
    public DateTime Timestamp { get; set; }
}