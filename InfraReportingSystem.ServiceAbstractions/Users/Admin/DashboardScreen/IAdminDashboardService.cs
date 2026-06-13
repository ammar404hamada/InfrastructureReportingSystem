using InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

namespace InfraReportingSystem.ServiceAbstractions.Users.Admin.DashboardScreen;

public interface IAdminDashboardService
{
    Task<DashboardResponseDto> GetDashboardAsync();
    Task<DashboardStatsDto> GetStatsAsync();
    Task<List<WeeklyTrafficDto>> GetWeeklyTrafficAsync();
    Task<List<CategoryStatDto>> GetCategoryStatsAsync();
    Task<List<LiveIssueDto>> GetLiveIssuesAsync();
    Task<List<RecentActionDto>> GetRecentActionsAsync();
}
