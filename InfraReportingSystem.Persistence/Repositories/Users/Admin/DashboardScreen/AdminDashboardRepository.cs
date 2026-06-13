using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.DashboardScreen;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Persistence.Repositories.Users.Admin.DashboardScreen;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _context;

    public AdminDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<int> GetActiveWorkerCountAsync()
    {
        return await _context.Workers
            .AsNoTracking()
            .CountAsync(w =>
                w.Status == UserStatus.Active &&
                w.AssignedReports.Any(r => r.Status == ReportStatus.InProgress));
    }

    public async Task<int> GetPendingIssuesCountAsync()
    {
        var pendingStatuses = new[]
        {
            ReportStatus.Submitted,
            ReportStatus.Assigned,
            ReportStatus.InProgress,
            ReportStatus.OnHold
        };

        return await _context.Reports
            .AsNoTracking()
            .CountAsync(r => pendingStatuses.Contains(r.Status));
    }

    public async Task<int> GetResolvedIssuesCountAsync()
    {
        return await _context.Reports
            .AsNoTracking()
            .CountAsync(r => r.Status == ReportStatus.Resolved);
    }

    public async Task<List<(string DayOfWeek, int ReportedCount, int ResolvedCount)>> GetWeeklyTrafficAsync(
        DateTime weekStart, DateTime weekEnd)
    {
        var reports = await _context.Reports
            .AsNoTracking()
            .Where(r => r.UploadedAt >= weekStart && r.UploadedAt < weekEnd)
            .Select(r => new { r.UploadedAt, r.Status })
            .ToListAsync();

        var days = new List<(string, int, int)>();

        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var dayReports = reports.Where(r => r.UploadedAt.Date == day.Date).ToList();

            days.Add((
                day.DayOfWeek.ToString(),
                dayReports.Count,
                dayReports.Count(r => r.Status == ReportStatus.Resolved)
            ));
        }

        return days;
    }

    public async Task<List<(string CategoryName, int TotalReported, int ResolvedCount)>> GetCategoryStatsAsync()
    {
        var data = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.Name,
                TotalReported = c.Reports.Count,
                ResolvedCount = c.Reports.Count(r => r.Status == ReportStatus.Resolved)
            })
            .ToListAsync();

        return data.Select(d => (d.Name, d.TotalReported, d.ResolvedCount)).ToList();
    }

    public async Task<List<Report>> GetLiveIssuesAsync()
    {
        return await _context.Reports
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.SubmittedBy)
            .Where(r => r.Status != ReportStatus.Resolved)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetRecentWorkerActionsAsync(int count)
    {
        var actionTypes = new[]
        {
            AuditActionType.WorkerAcceptedTask,
            AuditActionType.WorkerRejectedTask,
            AuditActionType.WorkerMarkedTaskAsFixed
        };

        return await _context.AuditLog
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => actionTypes.Contains(a.ActionType))
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
