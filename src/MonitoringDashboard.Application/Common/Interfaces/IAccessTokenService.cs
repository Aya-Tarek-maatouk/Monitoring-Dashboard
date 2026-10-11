using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IAccessTokenService
{
    AuthenticationResponse CreateToken(User user, string role, IReadOnlyCollection<string> permissions);
}