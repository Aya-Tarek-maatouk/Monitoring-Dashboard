using System.ComponentModel.DataAnnotations;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class ReportStorageOptions
{
    [Required]
    public string Directory { get; init; } = "reports";
}