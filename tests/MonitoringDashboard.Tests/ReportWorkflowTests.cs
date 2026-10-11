using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Reports;
using MonitoringDashboard.Application.Features.Reports.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Infrastructure.BackgroundJobs;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;
using MonitoringDashboard.Infrastructure.Services;

namespace MonitoringDashboard.Tests;

public sealed class ReportWorkflowTests
{
    [Fact]
    public async Task ReportFailures_ThrowTypedExceptionsWithoutCreatingRequests()
    {
        await using var context = CreateContext();
        var service = new ReportService(new ServerRepository(context), new ReportRepository(context),
            context, new RecordingReportJobScheduler(), new EmptyReportFileStore(), TimeProvider.System);
        var now = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<BadRequestException>(() => service.RequestAsync(999,
            new CreateReportRequest { StartTime = now, EndTime = now }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => service.RequestAsync(999,
            new CreateReportRequest { StartTime = now.AddHours(-1), EndTime = now }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => service.OpenDownloadAsync(999, CancellationToken.None));
        Assert.Empty(await context.Reports.ToListAsync());
    }

    [Fact]
    public async Task OpenDownloadAsync_RejectsPendingReportsAndMissingFiles()
    {
        await using var context = CreateContext();
        var report = new Report { ServerId = 1, Status = ReportStatus.Pending };
        context.Reports.Add(report);
        await context.SaveChangesAsync();
        var service = new ReportService(new ServerRepository(context), new ReportRepository(context),
            context, new RecordingReportJobScheduler(), new EmptyReportFileStore(), TimeProvider.System);

        await Assert.ThrowsAsync<ConflictException>(() => service.OpenDownloadAsync(report.ReportId, CancellationToken.None));
        report.Status = ReportStatus.Completed;
        report.FilePath = "missing.csv";
        await context.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => service.OpenDownloadAsync(report.ReportId, CancellationToken.None));
    }

    [Fact]
    public async Task RequestAsync_PersistsPendingReportAndEnqueuesGeneration()
    {
        await using var context = CreateContext();
        var server = new Server { Name = "report-server", CreatedAt = DateTime.UtcNow };
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        var scheduler = new RecordingReportJobScheduler();
        var service = new ReportService(
            new ServerRepository(context),
            new ReportRepository(context),
            context,
            scheduler,
            new EmptyReportFileStore(),
            TimeProvider.System);

        var response = await service.RequestAsync(server.ServerId, new CreateReportRequest
        {
            StartTime = DateTimeOffset.UtcNow.AddHours(-1),
            EndTime = DateTimeOffset.UtcNow
        }, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(ReportStatus.Pending, response.Status);
        Assert.False(response.HasDownload);
        Assert.Equal(response.ReportId, scheduler.ReportId);
        Assert.Equal(ReportStatus.Pending, (await context.Reports.SingleAsync()).Status);
    }

    [Fact]
    public async Task GenerateAsync_WritesCsvAndMarksReportCompleted()
    {
        await using var context = CreateContext();
        var server = new Server { Name = "report-server", CreatedAt = DateTime.UtcNow };
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        var report = new Report
        {
            ServerId = server.ServerId,
            StartTime = now.AddHours(-1),
            EndTime = now.AddHours(1),
            CreatedAt = now,
            Status = ReportStatus.Pending
        };
        context.Reports.Add(report);
        context.Metrics.Add(new Metric
        {
            ServerId = server.ServerId,
            CpuUsage = 25,
            MemoryUsage = 40,
            DiskUsage = 55,
            ResponseTime = 10,
            Status = ServerStatus.Up,
            Timestamp = now
        });
        await context.SaveChangesAsync();
        var reportDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var job = new ReportGenerationJob(
                new ReportRepository(context),
                new MetricRepository(context),
                context,
                Options.Create(new ReportStorageOptions { Directory = reportDirectory }),
                TimeProvider.System,
                NullLogger<ReportGenerationJob>.Instance);

            await job.GenerateAsync(report.ReportId, CancellationToken.None);

            var completed = await context.Reports.SingleAsync();
            Assert.Equal(ReportStatus.Completed, completed.Status);
            Assert.NotNull(completed.CompletedAt);
            Assert.True(File.Exists(completed.FilePath));
            var csv = await File.ReadAllTextAsync(completed.FilePath!);
            Assert.Contains("Timestamp,CpuUsage,MemoryUsage,DiskUsage,ResponseTime,Status", csv);
            Assert.Contains("25", csv);
        }
        finally
        {
            if (Directory.Exists(reportDirectory))
            {
                Directory.Delete(reportDirectory, recursive: true);
            }
        }
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class RecordingReportJobScheduler : IReportJobScheduler
    {
        public int ReportId { get; private set; }

        public string EnqueueReportGeneration(int reportId)
        {
            ReportId = reportId;
            return "test-job";
        }
    }

    private sealed class EmptyReportFileStore : IReportFileStore
    {
        public Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken) =>
            Task.FromResult<Stream?>(null);
    }
}