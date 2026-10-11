using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class JwtAccessTokenService(
    IOptions<JwtTokenOptions> options,
    TimeProvider timeProvider) : IAccessTokenService
{
    public AuthenticationResponse CreateToken(User user, string role, IReadOnlyCollection<string> permissions)
    {
        var settings = options.Value;
        var issuedAt = timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(settings.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };
        claims.Add(new Claim(ClaimTypes.Role, role));
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            credentials);

        return new AuthenticationResponse(
            user.UserId,
            user.UserName,
            role,
            permissions.ToArray(),
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}