using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksHistoryScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Worker.TasksHistoryScreen
{
    public class WorkerHistoryRepository : IWorkerHistoryRepository
    {
        private readonly AppDbContext _context;

        public WorkerHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Report> Items, int TotalCount)> GetHistoryAsync(
            string workerId,
            string? searchTerm,
            List<ReportStatus>? statusFilters,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.ReportPics)
                .Include(r => r.SubmittedBy)
                .Include(r => r.AssignedByAuthority)
                .Where(r => r.AssignedWorkerId == workerId);

            if (statusFilters != null && statusFilters.Any())
                query = query.Where(r => statusFilters.Contains(r.Status));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(r =>
                    EF.Functions.Like(r.Description, $"%{term}%") ||
                    EF.Functions.Like(r.Category.Name, $"%{term}%"));
            }

            var totalCount = await query.CountAsync();
            if (totalCount == 0) return (new List<Report>(), 0);

            var orderedQuery = query.Select(r => new
            {
                Report = r,
                LastUpdated = _context.AuditLog
                    .Where(a => a.EntityName == "Report" && a.EntityId == r.Id.ToString())
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => a.Timestamp)
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.LastUpdated)
            .Select(x => x.Report);

            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
