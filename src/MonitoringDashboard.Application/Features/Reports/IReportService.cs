using MonitoringDashboard.Application.Features.Reports.DTOs;

namespace MonitoringDashboard.Application.Features.Reports;

public interface IReportService
{
    Task<ReportResponse> RequestAsync(
        int serverId,
        CreateReportRequest request,
        CancellationToken cancellationToken);

    Task<ReportPageResponse> GetPageAsync(
        int? serverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ReportResponse> GetByIdAsync(int reportId, CancellationToken cancellationToken);
    Task<ReportDownload> OpenDownloadAsync(int reportId, CancellationToken cancellationToken);
}