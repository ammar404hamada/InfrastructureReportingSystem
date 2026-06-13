namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class WeeklyTrafficDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int ReportedCount { get; set; }
    public int ResolvedCount { get; set; }
}
