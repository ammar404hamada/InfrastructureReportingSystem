using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Worker.CurrentTaskScreen
{
    public class WorkerCurrentTaskActionsRepository : IWorkerCurrentTaskActionsRepository
    {
        private readonly AppDbContext _context;

        public WorkerCurrentTaskActionsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetCurrentTaskAsync(string workerId)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r =>
                    r.AssignedWorkerId == workerId &&
                    r.Status == ReportStatus.InProgress);
        }

        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}
