using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport;
using Microsoft.EntityFrameworkCore;

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
        await _context.AuditLog.AddAsync(auditLog);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}