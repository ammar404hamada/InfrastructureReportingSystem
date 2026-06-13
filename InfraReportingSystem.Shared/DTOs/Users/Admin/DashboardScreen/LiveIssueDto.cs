namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class LiveIssueDto
{
    public int ReportId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Priority { get; set; } = string.Empty;
}
