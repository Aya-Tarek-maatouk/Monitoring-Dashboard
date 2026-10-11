using Microsoft.Extensions.Logging;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public sealed class ReportNotificationJob(
    IReportRepository reports,
    ILogger<ReportNotificationJob> logger)
{
    public async Task NotifyReadyAsync(int reportId, CancellationToken cancellationToken)
    {
        var report = await reports.GetReadOnlyByIdAsync(reportId, cancellationToken);
        if (report?.Status != ReportStatus.Completed)
        {
            return;
        }
        // TO DO : Send email notification to the user that the report is ready for download
        logger.LogInformation(
            "report-ready email for report {ReportId} and server {ServerId}",
            report.ReportId,
            report.ServerId);
    }
}