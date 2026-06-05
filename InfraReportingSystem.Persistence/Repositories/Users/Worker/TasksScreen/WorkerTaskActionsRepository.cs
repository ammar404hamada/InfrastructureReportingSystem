using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Users.Worker.TasksScreen
{
    public class WorkerTaskActionsRepository : IWorkerTaskActionsRepository
    {
        private readonly AppDbContext _context;

        public WorkerTaskActionsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetByIdAsync(int reportId)
            => await _context.Reports.FindAsync(reportId);

        public async Task<bool> HasActiveTaskAsync(string workerId)
            => await _context.Reports.AnyAsync(r =>
                r.AssignedWorkerId == workerId &&
                r.Status == ReportStatus.InProgress);

        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}
