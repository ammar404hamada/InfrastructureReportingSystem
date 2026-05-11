using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.AlsoSuffer;

namespace InfraReportingSystem.Persistence.Repositories.PublicUser.AlsoSuffer
{
    public class PublicAlsoSufferRepository : IPublicAlsoSufferRepository
    {
        private readonly AppDbContext _context;

        public PublicAlsoSufferRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportStatus?> GetReportStatusAsync(int reportId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.Id == reportId)
                .Select(r => (ReportStatus?)r.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AlreadyAffectedAsync(int reportId, string userId)
        {
            return await _context.ReportAffectedUsers
                .AsNoTracking()
                .AnyAsync(a => a.ReportId == reportId && a.UserId == userId);
        }

        public async Task AddAsync(ReportAffectedUser entity)
        {
            await _context.ReportAffectedUsers.AddAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
