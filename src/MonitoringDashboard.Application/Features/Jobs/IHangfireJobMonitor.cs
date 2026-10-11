using MonitoringDashboard.Application.Features.Jobs.DTOs;

namespace MonitoringDashboard.Application.Features.Jobs;

public interface IHangfireJobMonitor
{
    BackgroundJobPageResponse GetPage(int page, int pageSize);
    RetryJobOutcome RetryFailedJob(string jobId);
}