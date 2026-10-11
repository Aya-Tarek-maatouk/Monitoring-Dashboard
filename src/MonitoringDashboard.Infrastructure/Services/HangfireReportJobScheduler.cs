using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.BackgroundJobs;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class HangfireReportJobScheduler(IBackgroundJobClient backgroundJobs) : IReportJobScheduler
{
    public string EnqueueReportGeneration(int reportId)
    {
        var generationJobId = backgroundJobs.Create(
            Job.FromExpression<ReportGenerationJob>(job => job.GenerateAsync(reportId, CancellationToken.None)),
            new EnqueuedState());

        backgroundJobs.Create(
            Job.FromExpression<ReportNotificationJob>(job => job.NotifyReadyAsync(reportId, CancellationToken.None)),
            new AwaitingState(generationJobId, new EnqueuedState()));

        return generationJobId;
    }
}