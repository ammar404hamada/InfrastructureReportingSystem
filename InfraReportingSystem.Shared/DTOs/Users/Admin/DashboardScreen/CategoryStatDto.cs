namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

public class CategoryStatDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalReported { get; set; }
    public int ResolvedCount { get; set; }
}
