namespace MonitoringDashboard.Application.Features.Jobs;

public enum RetryJobOutcome
{
    Requeued,
    NotFound,
    NotFailed
}