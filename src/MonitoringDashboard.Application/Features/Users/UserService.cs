using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Users;

public sealed class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IUserPasswordService passwordService,
    IAccessTokenService tokenService,
    TimeProvider timeProvider) : IUserService
{
    public async Task<AuthenticationResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var alreadyExists = await users.ExistsByUserNameOrEmailAsync(userName, email, cancellationToken);
        if (alreadyExists)
        {
            throw new ConflictException(ValidationMessages.UserAlreadyExists);
        }

        var role = await users.GetRoleWithPermissionsAsync("User", cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException(ValidationMessages.UserRoleMissing);
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            Role = role,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        user.PasswordHash = passwordService.Hash(user, request.Password);
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthenticationResponse(user);
    }

    public async Task<AuthenticationResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();
        var user = await users.GetWithRoleAndPermissionsAsync(identifier, normalizedEmail, cancellationToken);

        if (user is null || !passwordService.Verify(user, user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException(ValidationMessages.InvalidCredentials);
        }

        return CreateAuthenticationResponse(user);
    }

    public async Task<UserPageResponse> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await users.GetPageWithRolesAsync(page, pageSize, cancellationToken);

        var responses = items.Select(user => new UserSummaryResponse(
            user.UserId,
            user.UserName,
            user.Email,
            new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)),
            user.RoleId,
            user.Role.Name,
            GetPermissions(user.Role))).ToArray();

        return new UserPageResponse(responses, page, pageSize, totalCount);
    }

    public async Task<UserSummaryResponse> ChangeRoleAsync(
        int userId, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (request.RoleId <= 0)
        {
            throw new BadRequestException(ValidationMessages.RoleIdInvalid);
        }

        return await unitOfWork.ExecuteSerializableAsync(async token =>
        {
            var user = await users.GetByIdAsync(userId, token)
                ?? throw new NotFoundException(ValidationMessages.UserNotFound);
            var role = await users.GetRoleWithPermissionsByIdAsync(request.RoleId, token)
                ?? throw new NotFoundException(ValidationMessages.RoleNotFound);
            var currentRole = await users.GetRoleWithPermissionsByIdAsync(user.RoleId, token)
                ?? throw new InvalidOperationException(ValidationMessages.CurrentRoleMissing);
            if (GetPermissions(currentRole).Contains("users.manage") && !GetPermissions(role).Contains("users.manage")
                && await users.CountByPermissionAsync("users.manage", token) <= 1)
            {
                throw new ConflictException(ValidationMessages.LastAdminCannotBeDemoted);
            }

            user.Role = role;
            user.RoleId = role.RoleId;
            await unitOfWork.SaveChangesAsync(token);
            return new UserSummaryResponse(user.UserId, user.UserName, user.Email,
                new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)),
                role.RoleId, role.Name, GetPermissions(role));
        }, cancellationToken);
    }

    private AuthenticationResponse CreateAuthenticationResponse(User user)
    {
        return tokenService.CreateToken(user, user.Role.Name, GetPermissions(user.Role));
    }

    private static string[] GetPermissions(Role role) => role.RolePermissions
        .Select(rolePermission => rolePermission.Permission.Name)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(permission => permission, StringComparer.Ordinal)
        .ToArray();
}