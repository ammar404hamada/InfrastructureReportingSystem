using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Persistence.Repositories.Users.Worker.TasksScreen;

public class WorkerTasksRepository : IWorkerTasksRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkerTasksRepository> _logger;

    public WorkerTasksRepository(AppDbContext context, ILogger<WorkerTasksRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Report> Items, int TotalCount)> GetMyTasksAsync(
        string workerId,
        string? searchTerm,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Reports
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.ReportPics)
            .Include(r => r.SubmittedBy)
            .Include(r => r.AssignedByAuthority)
            .Where(r =>
                r.AssignedWorkerId == workerId &&
                (r.Status == ReportStatus.Assigned));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(r =>
                EF.Functions.Like(r.Description, $"%{term}%") ||
                EF.Functions.Like(r.Category.Name, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.AssignedAt == null)
            .ThenByDescending(r => r.AssignedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogDebug("Worker {WorkerId} has {TotalCount} total tasks after filtering.", workerId, totalCount);

        return (items, totalCount);
    }
}