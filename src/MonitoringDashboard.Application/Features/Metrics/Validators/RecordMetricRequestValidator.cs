using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Metrics.DTOs;

namespace MonitoringDashboard.Application.Features.Metrics.Validators;

public sealed class RecordMetricRequestValidator : AbstractValidator<RecordMetricRequest>
{
    public RecordMetricRequestValidator()
    {
        RuleFor(request => request.CpuUsage).Must(value => double.IsFinite(value) && value is >= 0 and <= 100)
            .WithMessage(_ => ValidationMessages.MetricUsageInvalid);
        RuleFor(request => request.MemoryUsage).Must(value => double.IsFinite(value) && value is >= 0 and <= 100)
            .WithMessage(_ => ValidationMessages.MetricUsageInvalid);
        RuleFor(request => request.DiskUsage).Must(value => double.IsFinite(value) && value is >= 0 and <= 100)
            .WithMessage(_ => ValidationMessages.MetricUsageInvalid);
        RuleFor(request => request.ResponseTime).Must(value => double.IsFinite(value) && value >= 0)
            .WithMessage(_ => ValidationMessages.ResponseTimeInvalid);
        RuleFor(request => request.Status).IsInEnum().WithMessage(_ => ValidationMessages.ServerStatusInvalid);
    }
}