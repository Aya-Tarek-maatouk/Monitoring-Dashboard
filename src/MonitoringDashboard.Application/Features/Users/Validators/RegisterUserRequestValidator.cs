using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Users.DTOs;

namespace MonitoringDashboard.Application.Features.Users.Validators;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(request => request.UserName).NotEmpty().WithMessage(_ => ValidationMessages.UserNameInvalid)
            .Length(3, 50).WithMessage(_ => ValidationMessages.UserNameInvalid);
        RuleFor(request => request.Email).NotEmpty().WithMessage(_ => ValidationMessages.EmailInvalid)
            .MaximumLength(100).WithMessage(_ => ValidationMessages.EmailInvalid)
            .EmailAddress().WithMessage(_ => ValidationMessages.EmailInvalid);
        RuleFor(request => request.Password).NotEmpty().WithMessage(_ => ValidationMessages.RegistrationPasswordInvalid)
            .Length(12, 128).WithMessage(_ => ValidationMessages.RegistrationPasswordInvalid);
    }
}