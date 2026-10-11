using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Users.DTOs;

namespace MonitoringDashboard.Application.Features.Users.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(request => request.UserNameOrEmail).NotEmpty().WithMessage(_ => ValidationMessages.LoginIdentifierInvalid)
            .MaximumLength(100).WithMessage(_ => ValidationMessages.LoginIdentifierInvalid);
        RuleFor(request => request.Password).NotEmpty().WithMessage(_ => ValidationMessages.LoginPasswordInvalid)
            .MaximumLength(128).WithMessage(_ => ValidationMessages.LoginPasswordInvalid);
    }
}