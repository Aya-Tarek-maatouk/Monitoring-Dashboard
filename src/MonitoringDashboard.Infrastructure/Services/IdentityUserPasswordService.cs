using Microsoft.AspNetCore.Identity;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class IdentityUserPasswordService(IPasswordHasher<User> passwordHasher) : IUserPasswordService
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, password)
        is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}