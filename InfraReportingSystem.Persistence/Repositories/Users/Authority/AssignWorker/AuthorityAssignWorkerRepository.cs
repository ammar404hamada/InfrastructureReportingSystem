using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.AssignWorker;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Persistence.Repositories.Users.Authority.AssignWorker
{
    public class AuthorityAssignWorkerRepository : IAuthorityAssignWorkerRepository
    {
        private readonly AppDbContext _context;

        public AuthorityAssignWorkerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetByIdAsync(int reportId)
        {
            return await _context.Reports
                .Include(r => r.SubmittedBy)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<bool> HasActiveTaskAsync(string workerId)
            => await _context.Reports.AnyAsync(r =>
                r.AssignedWorkerId == workerId &&
                r.Status == ReportStatus.InProgress);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}