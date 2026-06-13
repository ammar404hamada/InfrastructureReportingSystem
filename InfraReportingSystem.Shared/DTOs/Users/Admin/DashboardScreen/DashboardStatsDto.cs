namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveWorkers { get; set; }
    public int PendingIssues { get; set; }
    public int ResolvedIssues { get; set; }
}
