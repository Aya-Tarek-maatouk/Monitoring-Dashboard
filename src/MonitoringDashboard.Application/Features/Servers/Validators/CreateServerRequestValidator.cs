using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Servers.DTOs;

namespace MonitoringDashboard.Application.Features.Servers.Validators;

public sealed class CreateServerRequestValidator : AbstractValidator<CreateServerRequest>
{
    public CreateServerRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(request => request.Name).NotEmpty().WithMessage(_ => ValidationMessages.ServerNameInvalid)
            .MaximumLength(100).WithMessage(_ => ValidationMessages.ServerNameInvalid);
        RuleFor(request => request.IPAddress).MaximumLength(50).WithMessage(_ => ValidationMessages.ServerAddressInvalid);
        RuleFor(request => request.Description).MaximumLength(250).WithMessage(_ => ValidationMessages.ServerDescriptionInvalid);
        RuleFor(request => request.Status).IsInEnum().WithMessage(_ => ValidationMessages.ServerStatusInvalid);
    }
}