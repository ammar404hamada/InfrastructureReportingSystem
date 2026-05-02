using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.AssignWorker;

namespace InfraReportingSystem.Persistence.Repositories.Authority.AssignWorker
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
