namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class DashboardResponseDto
{
    public DashboardStatsDto Stats { get; set; } = null!;
    public List<WeeklyTrafficDto> WeeklyTraffic { get; set; } = new();
    public List<CategoryStatDto> CategoryStats { get; set; } = new();
    public List<LiveIssueDto> LiveIssues { get; set; } = new();
    public List<RecentActionDto> RecentActions { get; set; } = new();
}
