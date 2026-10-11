namespace MonitoringDashboard.Application.Features.Reports.DTOs;

public sealed record ReportDownload(string FileName, Stream Content);