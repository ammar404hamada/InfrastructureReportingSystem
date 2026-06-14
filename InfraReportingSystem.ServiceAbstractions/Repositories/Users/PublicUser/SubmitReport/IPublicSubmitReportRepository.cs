using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport
{
    public interface IPublicSubmitReportRepository
    {
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task AddReportAsync(Report report);
        Task AddReportPicAsync(ReportPic reportPic);
        Task AddAuditLogAsync(AuditLog auditLog);
        Task SaveChangesAsync();
    }
}
