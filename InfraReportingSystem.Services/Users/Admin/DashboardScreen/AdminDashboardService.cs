using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.DashboardScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.DashboardScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;

namespace InfraReportingSystem.Services.Users.Admin.DashboardScreen;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _repository;

    public AdminDashboardService(IAdminDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardResponseDto> GetDashboardAsync()
    {
        var stats = await GetStatsAsync();
        var weekly = await GetWeeklyTrafficAsync();
        var categories = await GetCategoryStatsAsync();
        var live = await GetLiveIssuesAsync();
        var recent = await GetRecentActionsAsync();

        return new DashboardResponseDto
        {
            Stats = stats,
            WeeklyTraffic = weekly,
            CategoryStats = categories,
            LiveIssues = live,
            RecentActions = recent
        };
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalUsers = await _repository.GetTotalUsersCountAsync();
        var activeWorkers = await _repository.GetActiveWorkerCountAsync();
        var pending = await _repository.GetPendingIssuesCountAsync();
        var resolved = await _repository.GetResolvedIssuesCountAsync();

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveWorkers = activeWorkers,
            PendingIssues = pending,
            ResolvedIssues = resolved
        };
    }

    public async Task<List<WeeklyTrafficDto>> GetWeeklyTrafficAsync()
    {
        var now = DateTime.UtcNow;
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var weekStart = now.AddDays(-diff).Date;
        var weekEnd = weekStart.AddDays(7);

        var data = await _repository.GetWeeklyTrafficAsync(weekStart, weekEnd);

        return data.Select(d => new WeeklyTrafficDto
        {
            DayOfWeek = d.DayOfWeek,
            ReportedCount = d.ReportedCount,
            ResolvedCount = d.ResolvedCount
        }).ToList();
    }

    public async Task<List<CategoryStatDto>> GetCategoryStatsAsync()
    {
        var data = await _repository.GetCategoryStatsAsync();

        return data.Select(d => new CategoryStatDto
        {
            CategoryName = d.CategoryName,
            TotalReported = d.TotalReported,
            ResolvedCount = d.ResolvedCount
        }).ToList();
    }

    public async Task<List<LiveIssueDto>> GetLiveIssuesAsync()
    {
        var reports = await _repository.GetLiveIssuesAsync();
        var threshold = DateTime.UtcNow.AddDays(-7);

        return reports.Select(MapToLiveIssueDto).ToList();

        LiveIssueDto MapToLiveIssueDto(Report r)
        {
            var priority = r.UploadedAt < threshold ? "High" : "Normal";

            return new LiveIssueDto
            {
                ReportId = r.Id,
                CategoryName = r.Category.Name,
                ReporterName = r.SubmittedBy.Name,
                Status = r.Status.ToString(),
                SubmittedAt = r.UploadedAt,
                Priority = priority
            };
        }
    }

    public async Task<List<RecentActionDto>> GetRecentActionsAsync()
    {
        var logs = await _repository.GetRecentWorkerActionsAsync(10);

        return logs.Select(log => new RecentActionDto
        {
            WorkerName = log.User?.Name ?? "Unknown",
            ActionType = log.ActionType.ToString(),
            TargetEntity = !string.IsNullOrWhiteSpace(log.EntityName) &&
                           !string.IsNullOrWhiteSpace(log.EntityId)
                ? $"{log.EntityName} {log.EntityId}"
                : "System",
            Timestamp = log.Timestamp
        }).ToList();
    }
}
