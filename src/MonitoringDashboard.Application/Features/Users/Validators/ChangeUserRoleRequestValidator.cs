using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Users.DTOs;

namespace MonitoringDashboard.Application.Features.Users.Validators;

public sealed class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
{
    public ChangeUserRoleRequestValidator()
    {
        RuleFor(request => request.RoleId).GreaterThan(0).WithMessage(_ => ValidationMessages.RoleIdInvalid);
    }
}