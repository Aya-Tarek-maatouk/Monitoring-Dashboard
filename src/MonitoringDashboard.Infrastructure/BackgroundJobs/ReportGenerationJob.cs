using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Services;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public sealed class ReportGenerationJob(
    IReportRepository reports,
    IMetricRepository metricsRepository,
    IUnitOfWork unitOfWork,
    IOptions<ReportStorageOptions> storageOptions,
    TimeProvider timeProvider,
    ILogger<ReportGenerationJob> logger)
{
    public async Task GenerateAsync(int reportId, CancellationToken cancellationToken)
    {
        var report = await reports.GetByIdAsync(reportId, cancellationToken);
        if (report is null)
        {
            logger.LogWarning("Report {ReportId} was not found for generation", reportId);
            return;
        }

        report.Status = ReportStatus.Processing;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rootPath = Path.GetFullPath(storageOptions.Value.Directory, AppContext.BaseDirectory);
        var reportPath = Path.Combine(rootPath, $"report-{report.ReportId}.csv");
        var temporaryPath = reportPath + ".tmp";

        try
        {
            Directory.CreateDirectory(rootPath);
            var metrics = await metricsRepository.GetRangeAsync(
                report.ServerId,
                report.StartTime,
                report.EndTime,
                cancellationToken);

            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                useAsync: true))
            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("Timestamp,CpuUsage,MemoryUsage,DiskUsage,ResponseTime,Status");
                foreach (var metric in metrics)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var timestamp = DateTime.SpecifyKind(metric.Timestamp, DateTimeKind.Utc)
                        .ToString("O", CultureInfo.InvariantCulture);
                    var row = string.Join(',',
                        timestamp,
                        metric.CpuUsage.ToString(CultureInfo.InvariantCulture),
                        metric.MemoryUsage.ToString(CultureInfo.InvariantCulture),
                        metric.DiskUsage.ToString(CultureInfo.InvariantCulture),
                        metric.ResponseTime.ToString(CultureInfo.InvariantCulture),
                        metric.Status.ToString());
                    await writer.WriteLineAsync(row);
                }
            }

            File.Move(temporaryPath, reportPath, overwrite: true);
            report.FilePath = reportPath;
            report.Status = ReportStatus.Completed;
            report.CompletedAt = timeProvider.GetUtcNow().UtcDateTime;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Generated report {ReportId} with {MetricCount} metric samples", reportId, metrics.Count);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            report.Status = ReportStatus.Failed;
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }
}