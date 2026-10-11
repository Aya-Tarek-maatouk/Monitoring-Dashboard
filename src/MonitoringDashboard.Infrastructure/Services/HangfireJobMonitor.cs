using Hangfire;
using Hangfire.States;
using Hangfire.Storage.Monitoring;
using MonitoringDashboard.Application.Features.Jobs;
using MonitoringDashboard.Application.Features.Jobs.DTOs;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class HangfireJobMonitor(IBackgroundJobClient backgroundJobs) : IHangfireJobMonitor
{
    public BackgroundJobPageResponse GetPage(int page, int pageSize)
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var statistics = monitoring.GetStatistics();
        var scanCount = checked(page * pageSize);
        var offset = 0;
        var jobs = new List<BackgroundJobResponse>();

        jobs.AddRange(monitoring.FailedJobs(offset, scanCount)
            .Select(entry => Map(entry.Key, entry.Value)));
        jobs.AddRange(monitoring.ProcessingJobs(offset, scanCount)
            .Select(entry => Map(entry.Key, entry.Value)));
        jobs.AddRange(monitoring.ScheduledJobs(offset, scanCount)
            .Select(entry => Map(entry.Key, entry.Value)));
        jobs.AddRange(monitoring.EnqueuedJobs("default", offset, scanCount)
            .Select(entry => Map(entry.Key, entry.Value)));
        jobs.AddRange(monitoring.SucceededJobs(offset, scanCount)
            .Select(entry => Map(entry.Key, entry.Value)));

        var counts = new BackgroundJobCounts(
            statistics.Enqueued,
            statistics.Scheduled,
            statistics.Processing,
            monitoring.SucceededListCount(),
            monitoring.FailedCount());
        var items = jobs
            .OrderByDescending(job => job.StateChangedAt)
            .ThenByDescending(job => job.JobId, StringComparer.Ordinal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return new BackgroundJobPageResponse(
            items,
            page,
            pageSize,
            counts.Enqueued + counts.Scheduled + counts.Processing + counts.Succeeded + counts.Failed,
            counts,
            DateTimeOffset.UtcNow);
    }

    public RetryJobOutcome RetryFailedJob(string jobId)
    {
        var details = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
        if (details is null)
        {
            return RetryJobOutcome.NotFound;
        }

        if (!details.Properties.TryGetValue("State", out var currentState)
            || !string.Equals(currentState, FailedState.StateName, StringComparison.Ordinal))
        {
            return RetryJobOutcome.NotFailed;
        }

        return backgroundJobs.ChangeState(jobId, new EnqueuedState(), FailedState.StateName)
            ? RetryJobOutcome.Requeued
            : RetryJobOutcome.NotFailed;
    }

    private static BackgroundJobResponse Map(string jobId, FailedJobDto job) => new(
        jobId,
        GetJobName(job.Job, job.InvocationData),
        "Failed",
        AsUtc(job.FailedAt),
        job.ExceptionMessage ?? job.Reason,
        true);

    private static BackgroundJobResponse Map(string jobId, ProcessingJobDto job) => new(
        jobId,
        GetJobName(job.Job, job.InvocationData),
        "Processing",
        AsUtc(job.StartedAt),
        null,
        false);

    private static BackgroundJobResponse Map(string jobId, ScheduledJobDto job) => new(
        jobId,
        GetJobName(job.Job, job.InvocationData),
        "Scheduled",
        AsUtc(job.ScheduledAt),
        null,
        false);

    private static BackgroundJobResponse Map(string jobId, EnqueuedJobDto job) => new(
        jobId,
        GetJobName(job.Job, job.InvocationData),
        "Enqueued",
        AsUtc(job.EnqueuedAt),
        null,
        false);

    private static BackgroundJobResponse Map(string jobId, SucceededJobDto job) => new(
        jobId,
        GetJobName(job.Job, job.InvocationData),
        "Succeeded",
        AsUtc(job.SucceededAt),
        null,
        false);

    private static string GetJobName(Hangfire.Common.Job? job, Hangfire.Storage.InvocationData? invocation) =>
        job?.Method.Name ?? invocation?.Method ?? "Unknown job";

    private static DateTimeOffset? AsUtc(DateTime? value) => value.HasValue
        ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
        : null;
}