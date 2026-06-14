using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport;

namespace InfraReportingSystem.Persistence.Repositories.Users.PublicUser.SubmitReport
{
    public class PublicSubmitReportRepository : IPublicSubmitReportRepository
    {
        private readonly AppDbContext _context;

        public PublicSubmitReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task AddReportAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
        }

        public async Task AddReportPicAsync(ReportPic reportPic)
        {
            await _context.ReportPics.AddAsync(reportPic);
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
