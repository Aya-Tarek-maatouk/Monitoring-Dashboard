using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Services;

namespace MonitoringDashboard.Tests;

public sealed class SecurityImplementationTests
{
    [Fact]
    public void DemoAdminSeedHash_IsCompatibleWithIdentityPasswordHasher()
    {
        const string hash = "AQAAAAIAAYagAAAAEIvV2ogrzfdSNuA/xkQhMxeMTtwKNA3CMBDFpjc5HH9J/BJYk86/SFfDN6Oik7xhwA==";
        var service = new IdentityUserPasswordService(new PasswordHasher<User>());
        var user = new User { UserName = "demo.admin" };

        Assert.True(service.Verify(user, hash, "DemoAdmin!2026"));
        Assert.False(service.Verify(user, hash, "incorrect password"));
    }

    [Fact]
    public void IdentityPasswordService_HashesAndVerifiesPasswords()
    {
        var service = new IdentityUserPasswordService(new PasswordHasher<User>());
        var user = new User { UserName = "user" };

        var hash = service.Hash(user, "strong test password");

        Assert.NotEqual("strong test password", hash);
        Assert.True(service.Verify(user, hash, "strong test password"));
        Assert.False(service.Verify(user, hash, "wrong password"));
    }

    [Fact]
    public void JwtTokenService_EmitsValidatedRoleClaims()
    {
        const string secret = "test-only-key-with-at-least-thirty-two-bytes";
        var settings = new JwtTokenOptions
        {
            Secret = secret,
            Issuer = "MonitoringDashboard.Tests",
            Audience = "MonitoringDashboard.Tests.Client",
            ExpirationMinutes = 30
        };
        var service = new JwtAccessTokenService(Options.Create(settings), TimeProvider.System);
        var response = service.CreateToken(
            new User { UserId = 42, UserName = "test-user", Email = "test@example.test" },
            "Admin",
            ["users.manage"]);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };

        var principal = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ValidateToken(response.AccessToken, validationParameters, out _);

        Assert.Equal("42", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Admin", principal.FindFirstValue(ClaimTypes.Role));
        Assert.Equal("users.manage", principal.FindFirstValue("permission"));
    }
}