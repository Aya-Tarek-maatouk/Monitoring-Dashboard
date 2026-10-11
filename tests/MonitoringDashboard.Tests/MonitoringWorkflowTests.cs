using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Alerts;
using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Application.Features.Metrics;
using MonitoringDashboard.Application.Features.Metrics.DTOs;
using MonitoringDashboard.Application.Features.Servers;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;

namespace MonitoringDashboard.Tests;

public sealed class MonitoringWorkflowTests
{
    [Fact]
    public async Task MissingServer_ThrowsNotFoundForLookupsAndMutations()
    {
        await using var context = CreateContext();
        var servers = CreateServerService(context);
        var metrics = CreateMetricService(context, new RecordingPublisher());

        await Assert.ThrowsAsync<NotFoundException>(() => servers.GetByIdAsync(999, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => servers.DeleteAsync(999, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => metrics.RecordAsync(999, CreateRequest(20), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => metrics.GetHistoryAsync(999, 1, 10, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task RecordAsync_WhenThresholdIsExceeded_CreatesAndPublishesAlert()
    {
        await using var context = CreateContext();
        var server = await AddServerAsync(context);
        var publisher = new RecordingPublisher();
        var service = CreateMetricService(context, publisher);

        var metric = await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 85), CancellationToken.None);

        Assert.NotNull(metric);
        Assert.Equal(ServerStatus.Up, (await context.Servers.SingleAsync()).Status);
        Assert.Single(await context.Metrics.ToListAsync());
        var alert = Assert.Single(await context.Alerts.ToListAsync());
        Assert.Equal("CpuUsage", alert.MetricType);
        Assert.Equal(AlertStatus.Triggered, alert.Status);
        Assert.Single(publisher.Metrics);
        Assert.Single(publisher.RaisedAlerts);
        Assert.Empty(publisher.ResolvedAlerts);
    }

    [Fact]
    public async Task RecordAsync_OnServerDown_SchedulesOneDelayedCheckAndResolvesOnRecovery()
    {
        await using var context = CreateContext();
        var server = await AddServerAsync(context);
        var publisher = new RecordingPublisher();
        var delayedScheduler = new RecordingDelayedAlertScheduler();
        var service = CreateMetricService(context, publisher, delayedScheduler);

        await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 20, ServerStatus.Down), CancellationToken.None);
        await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 20, ServerStatus.Down), CancellationToken.None);

        Assert.Single(delayedScheduler.ScheduledChecks);
        Assert.Equal(TimeSpan.FromMinutes(5), delayedScheduler.ScheduledChecks[0].Delay);
        var statusAlert = Assert.Single(await context.Alerts.ToListAsync());
        Assert.Equal("ServerStatus", statusAlert.MetricType);
        Assert.Equal(AlertStatus.Triggered, statusAlert.Status);

        await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 20, ServerStatus.Up), CancellationToken.None);

        Assert.Equal(AlertStatus.Resolved, statusAlert.Status);
        Assert.Single(publisher.ResolvedAlerts);
    }

    [Fact]
    public async Task RecordAsync_WhenMetricReturnsBelowThreshold_ResolvesExistingAlert()
    {
        await using var context = CreateContext();
        var server = await AddServerAsync(context);
        var publisher = new RecordingPublisher();
        var service = CreateMetricService(context, publisher);

        await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 85), CancellationToken.None);
        await service.RecordAsync(server.ServerId, CreateRequest(cpuUsage: 40), CancellationToken.None);

        var alert = Assert.Single(await context.Alerts.ToListAsync());
        Assert.Equal(AlertStatus.Resolved, alert.Status);
        Assert.NotNull(alert.ResolvedAt);
        Assert.Single(publisher.RaisedAlerts);
        Assert.Single(publisher.ResolvedAlerts);
        Assert.Equal(2, publisher.Metrics.Count);
    }

    [Fact]
    public async Task DeleteAsync_WhenServerHasMetricHistory_ThrowsConflict()
    {
        await using var context = CreateContext();
        var server = await AddServerAsync(context);
        context.Metrics.Add(new Metric
        {
            ServerId = server.ServerId,
            CpuUsage = 10,
            MemoryUsage = 20,
            DiskUsage = 30,
            ResponseTime = 5,
            Status = ServerStatus.Up,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateServerService(context);
        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(server.ServerId, CancellationToken.None));
        Assert.True(await context.Servers.AnyAsync(item => item.ServerId == server.ServerId));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<Server> AddServerAsync(ApplicationDbContext context)
    {
        var server = new Server { Name = "test-server", CreatedAt = DateTime.UtcNow };
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        return server;
    }

    private static MetricService CreateMetricService(
        ApplicationDbContext context,
        RecordingPublisher publisher,
        RecordingDelayedAlertScheduler? delayedScheduler = null) => new(
            new ServerRepository(context),
            new MetricRepository(context),
            new AlertRepository(context),
            context,
            publisher,
            delayedScheduler ?? new RecordingDelayedAlertScheduler(),
            Options.Create(new MonitoringThresholds
            {
                CpuUsage = 80,
                MemoryUsage = 90,
                DiskUsage = 90
            }),
            TimeProvider.System);

    private static ServerService CreateServerService(ApplicationDbContext context) => new(
        new ServerRepository(context),
        new MetricRepository(context),
        new AlertRepository(context),
        new ReportRepository(context),
        context,
        TimeProvider.System);

    private static RecordMetricRequest CreateRequest(double cpuUsage, ServerStatus status = ServerStatus.Up) => new()
    {
        CpuUsage = cpuUsage,
        MemoryUsage = 40,
        DiskUsage = 50,
        ResponseTime = 12,
        Status = status
    };

    private sealed class RecordingDelayedAlertScheduler : IDelayedAlertScheduler
    {
        public List<(int ServerId, TimeSpan Delay)> ScheduledChecks { get; } = [];

        public void ScheduleServerHealthCheck(int serverId, TimeSpan delay) =>
            ScheduledChecks.Add((serverId, delay));
    }

    private sealed class RecordingPublisher : IMonitoringUpdatePublisher
    {
        public List<MetricResponse> Metrics { get; } = [];
        public List<AlertResponse> RaisedAlerts { get; } = [];
        public List<AlertResponse> ResolvedAlerts { get; } = [];

        public Task PublishMetricAsync(int serverId, MetricResponse metric, CancellationToken cancellationToken)
        {
            Metrics.Add(metric);
            return Task.CompletedTask;
        }

        public Task PublishAlertRaisedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken)
        {
            RaisedAlerts.Add(alert);
            return Task.CompletedTask;
        }

        public Task PublishAlertResolvedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken)
        {
            ResolvedAlerts.Add(alert);
            return Task.CompletedTask;
        }
    }
}