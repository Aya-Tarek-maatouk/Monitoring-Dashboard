using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IUserPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string password);
}