namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IDelayedAlertScheduler
{
    void ScheduleServerHealthCheck(int serverId, TimeSpan delay);
}