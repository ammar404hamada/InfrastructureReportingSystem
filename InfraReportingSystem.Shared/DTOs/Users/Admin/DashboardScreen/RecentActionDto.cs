namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class RecentActionDto
{
    public string WorkerName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
