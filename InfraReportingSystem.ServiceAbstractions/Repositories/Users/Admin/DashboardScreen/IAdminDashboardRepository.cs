using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.DashboardScreen;

public interface IAdminDashboardRepository
{
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveWorkerCountAsync();
    Task<int> GetPendingIssuesCountAsync();
    Task<int> GetResolvedIssuesCountAsync();
    Task<List<(string DayOfWeek, int ReportedCount, int ResolvedCount)>> GetWeeklyTrafficAsync(DateTime weekStart, DateTime weekEnd);
    Task<List<(string CategoryName, int TotalReported, int ResolvedCount)>> GetCategoryStatsAsync();
    Task<List<Report>> GetLiveIssuesAsync();
    Task<List<AuditLog>> GetRecentWorkerActionsAsync(int count);
}
