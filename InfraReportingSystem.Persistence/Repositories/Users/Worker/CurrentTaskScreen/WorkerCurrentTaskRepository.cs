using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.CurrentTaskScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Users.Worker.CurrentTaskScreen
{
    public class WorkerCurrentTaskRepository : IWorkerCurrentTaskRepository
    {
        private readonly AppDbContext _context;

        public WorkerCurrentTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetCurrentTaskAsync(string workerId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.ReportPics)
                .Include(r => r.SubmittedBy)
                .Include(r => r.AssignedByAuthority)
                .FirstOrDefaultAsync(r =>
                    r.AssignedWorkerId == workerId &&
                    r.Status == ReportStatus.InProgress);
        }
    }
}
