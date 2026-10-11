using Microsoft.Extensions.Options;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Application.Features.Metrics.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Metrics;

public sealed class MetricService(
    IServerRepository servers,
    IMetricRepository metrics,
    IAlertRepository alerts,
    IUnitOfWork unitOfWork,
    IMonitoringUpdatePublisher publisher,
    IDelayedAlertScheduler delayedAlertScheduler,
    IOptions<MonitoringThresholds> thresholds,
    TimeProvider timeProvider) : IMetricService
{
    public async Task<MetricResponse> RecordAsync(
        int serverId,
        RecordMetricRequest request,
        CancellationToken cancellationToken)
    {
        var server = await servers.GetByIdAsync(serverId, cancellationToken);
        if (server is null)
        {
            throw new NotFoundException(ValidationMessages.ServerNotFound);
        }

        var now = timeProvider.GetUtcNow();
        var metric = new Metric
        {
            ServerId = serverId,
            CpuUsage = request.CpuUsage,
            MemoryUsage = request.MemoryUsage,
            DiskUsage = request.DiskUsage,
            ResponseTime = request.ResponseTime,
            Status = request.Status,
            Timestamp = now.UtcDateTime
        };
        server.Status = request.Status;
        metrics.Add(metric);

        var activeAlerts = await alerts.GetActiveForServerAsync(serverId, cancellationToken);
        var raised = new List<Alert>();
        var resolved = new List<Alert>();

        EvaluateThreshold(activeAlerts, raised, resolved, serverId, "CpuUsage", request.CpuUsage, thresholds.Value.CpuUsage, now);
        EvaluateThreshold(activeAlerts, raised, resolved, serverId, "MemoryUsage", request.MemoryUsage, thresholds.Value.MemoryUsage, now);
        EvaluateThreshold(activeAlerts, raised, resolved, serverId, "DiskUsage", request.DiskUsage, thresholds.Value.DiskUsage, now);
        var statusAlertRaised = EvaluateServerStatus(activeAlerts, raised, resolved, serverId, request.Status, now);
        alerts.AddRange(raised);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (statusAlertRaised)
        {
            delayedAlertScheduler.ScheduleServerHealthCheck(serverId, TimeSpan.FromMinutes(5));
        }

        var response = ToResponse(metric);
        await publisher.PublishMetricAsync(serverId, response, cancellationToken);
        foreach (var alert in raised)
        {
            await publisher.PublishAlertRaisedAsync(serverId, ToAlertResponse(alert), cancellationToken);
        }

        foreach (var alert in resolved)
        {
            await publisher.PublishAlertResolvedAsync(serverId, ToAlertResponse(alert), cancellationToken);
        }

        return response;
    }

    public async Task<MetricHistoryResponse> GetHistoryAsync(
        int serverId,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        if (!await servers.ExistsAsync(serverId, cancellationToken))
        {
            throw new NotFoundException(ValidationMessages.ServerNotFound);
        }

        var (items, totalCount) = await metrics.GetPageAsync(serverId, page, pageSize, from, to, cancellationToken);

        return new MetricHistoryResponse(items.Select(ToResponse).ToArray(), page, pageSize, totalCount);
    }

    private static void EvaluateThreshold(
        IReadOnlyCollection<Alert> activeAlerts,
        ICollection<Alert> raised,
        ICollection<Alert> resolved,
        int serverId,
        string metricType,
        double value,
        double threshold,
        DateTimeOffset now)
    {
        var activeAlert = activeAlerts.FirstOrDefault(alert => alert.MetricType == metricType);
        if (value > threshold)
        {
            if (activeAlert is null)
            {
                raised.Add(new Alert
                {
                    ServerId = serverId,
                    MetricType = metricType,
                    MetricValue = value,
                    Threshold = threshold,
                    Status = AlertStatus.Triggered,
                    CreatedAt = now.UtcDateTime
                });
            }
            else
            {
                activeAlert.MetricValue = value;
            }

            return;
        }

        if (activeAlert is not null)
        {
            activeAlert.Status = AlertStatus.Resolved;
            activeAlert.ResolvedAt = now.UtcDateTime;
            resolved.Add(activeAlert);
        }
    }

    private static bool EvaluateServerStatus(
        IReadOnlyCollection<Alert> activeAlerts,
        ICollection<Alert> raised,
        ICollection<Alert> resolved,
        int serverId,
        ServerStatus status,
        DateTimeOffset now)
    {
        var activeAlert = activeAlerts.FirstOrDefault(alert => alert.MetricType == "ServerStatus");
        if (status == ServerStatus.Down)
        {
            if (activeAlert is not null)
            {
                activeAlert.MetricValue = 1;
                return false;
            }

            raised.Add(new Alert
            {
                ServerId = serverId,
                MetricType = "ServerStatus",
                MetricValue = 1,
                Threshold = 0,
                Status = AlertStatus.Triggered,
                CreatedAt = now.UtcDateTime
            });
            return true;
        }

        if (activeAlert is not null)
        {
            activeAlert.Status = AlertStatus.Resolved;
            activeAlert.ResolvedAt = now.UtcDateTime;
            resolved.Add(activeAlert);
        }

        return false;
    }

    private static MetricResponse ToResponse(Metric metric) => new(
        metric.MetricId,
        metric.ServerId,
        metric.CpuUsage,
        metric.MemoryUsage,
        metric.DiskUsage,
        metric.ResponseTime,
        metric.Status,
        new DateTimeOffset(DateTime.SpecifyKind(metric.Timestamp, DateTimeKind.Utc)));

    private static AlertResponse ToAlertResponse(Alert alert) => new(
        alert.AlertId,
        alert.ServerId,
        alert.MetricType,
        alert.MetricValue,
        alert.Threshold,
        alert.Status,
        new DateTimeOffset(DateTime.SpecifyKind(alert.CreatedAt, DateTimeKind.Utc)),
        alert.ResolvedAt.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(alert.ResolvedAt.Value, DateTimeKind.Utc))
            : null);
}