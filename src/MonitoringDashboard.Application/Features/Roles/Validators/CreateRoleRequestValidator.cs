using FluentValidation;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Roles.DTOs;

namespace MonitoringDashboard.Application.Features.Roles.Validators;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage(_ => ValidationMessages.RoleNameInvalid)
            .MaximumLength(50).WithMessage(_ => ValidationMessages.RoleNameInvalid);
        RuleFor(request => request.Description).MaximumLength(250).WithMessage(_ => ValidationMessages.RoleDescriptionInvalid);
        RuleFor(request => request.PermissionIds).NotNull().WithMessage(_ => ValidationMessages.PermissionIdsInvalid);
        RuleForEach(request => request.PermissionIds).GreaterThan(0).WithMessage(_ => ValidationMessages.PermissionIdsInvalid);
    }
}