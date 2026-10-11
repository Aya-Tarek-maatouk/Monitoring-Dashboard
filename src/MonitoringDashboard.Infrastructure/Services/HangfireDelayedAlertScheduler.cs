using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.BackgroundJobs;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class HangfireDelayedAlertScheduler(IBackgroundJobClient backgroundJobs) : IDelayedAlertScheduler
{
    public void ScheduleServerHealthCheck(int serverId, TimeSpan delay)
    {
        backgroundJobs.Create(
            Job.FromExpression<DelayedServerHealthCheckJob>(job => job.CheckAsync(serverId, CancellationToken.None)),
            new ScheduledState(delay));
    }
}