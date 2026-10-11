namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IReportJobScheduler
{
    string EnqueueReportGeneration(int reportId);
}