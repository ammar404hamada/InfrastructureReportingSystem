using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories
{
    public class WorkerTasksRepository : IWorkerTasksRepository
    {
        private readonly AppDbContext _context;

        public WorkerTasksRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetMyTasksAsync(string workerId, string? searchTerm)
        {
            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.ReportPics)
                .Include(r => r.SubmittedBy)
                .Include(r => r.AssignedByAuthority)
                .Where(r =>
                    r.AssignedWorkerId == workerId &&
                    (r.Status == ReportStatus.Assigned || r.Status == ReportStatus.Rejected));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                query = query.Where(r =>
                    r.Description.Contains(term) ||
                    r.Category.Name.Contains(term));
            }

            query = query
                .OrderByDescending(r => r.AssignedAt == null)
                .ThenByDescending(r => r.AssignedAt);

            return await query.ToListAsync();
        }
    }
}
