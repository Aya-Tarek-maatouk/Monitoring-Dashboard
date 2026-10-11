using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Reports.DTOs;

namespace MonitoringDashboard.Application.Features.Reports.Validators;

public sealed class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(request => request.StartTime).LessThan(request => request.EndTime)
            .WithMessage(_ => ValidationMessages.ReportTimeRangeInvalid);
    }
}