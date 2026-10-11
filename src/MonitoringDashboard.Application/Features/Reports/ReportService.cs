using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Reports.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Reports;

public sealed class ReportService(
    IServerRepository servers,
    IReportRepository reports,
    IUnitOfWork unitOfWork,
    IReportJobScheduler jobScheduler,
    IReportFileStore fileStore,
    TimeProvider timeProvider) : IReportService
{
    public async Task<ReportResponse> RequestAsync(
        int serverId,
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException(ValidationMessages.ReportTimeRangeInvalid);
        }

        if (!await servers.ExistsAsync(serverId, cancellationToken))
        {
            throw new NotFoundException(ValidationMessages.ServerNotFound);
        }

        var report = new Report
        {
            ServerId = serverId,
            StartTime = request.StartTime.UtcDateTime,
            EndTime = request.EndTime.UtcDateTime,
            Status = ReportStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        reports.Add(report);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            jobScheduler.EnqueueReportGeneration(report.ReportId);
        }
        catch
        {
            report.Status = ReportStatus.Failed;
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        return ToResponse(report);
    }

    public async Task<ReportPageResponse> GetPageAsync(
        int? serverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await reports.GetPageAsync(serverId, page, pageSize, cancellationToken);

        return new ReportPageResponse(items.Select(ToResponse).ToArray(), page, pageSize, totalCount);
    }

    public async Task<ReportResponse> GetByIdAsync(int reportId, CancellationToken cancellationToken)
    {
        var report = await reports.GetReadOnlyByIdAsync(reportId, cancellationToken);

        return report is null ? throw new NotFoundException(ValidationMessages.ReportNotFound) : ToResponse(report);
    }

    public async Task<ReportDownload> OpenDownloadAsync(int reportId, CancellationToken cancellationToken)
    {
        var report = await reports.GetReadOnlyByIdAsync(reportId, cancellationToken);
        if (report is null)
        {
            throw new NotFoundException(ValidationMessages.ReportNotFound);
        }

        if (report.Status != ReportStatus.Completed || string.IsNullOrWhiteSpace(report.FilePath))
        {
            throw new ConflictException(ValidationMessages.ReportNotReady);
        }

        var stream = await fileStore.OpenReadAsync(report.FilePath, cancellationToken);
        return stream is null
            ? throw new NotFoundException(ValidationMessages.ReportFileMissing)
            : new ReportDownload($"report-{reportId}.csv", stream);
    }

    private static ReportResponse ToResponse(Report report) => new(
        report.ReportId,
        report.ServerId,
        new DateTimeOffset(DateTime.SpecifyKind(report.StartTime, DateTimeKind.Utc)),
        new DateTimeOffset(DateTime.SpecifyKind(report.EndTime, DateTimeKind.Utc)),
        report.Status,
        report.Status == ReportStatus.Completed && !string.IsNullOrWhiteSpace(report.FilePath),
        new DateTimeOffset(DateTime.SpecifyKind(report.CreatedAt, DateTimeKind.Utc)),
        report.CompletedAt.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(report.CompletedAt.Value, DateTimeKind.Utc))
            : null);
}