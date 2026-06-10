using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Persistence.Repositories.Users.PublicUser.MyReports
{
    public class PublicUserReportsRepository : IPublicUserReportsRepository
    {
        private readonly AppDbContext _context;

        public PublicUserReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId)
        {
            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => r.SubmittedById == userId)
                .Include(r => r.Category)
                .Include(r => r.AssignedByAuthority)
                .Include(r => r.AssignedWorker)
                .Include(r => r.ReportPics)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => new PublicUserReportsDto
                {
                    ReportId        = r.Id,
                    Title           = r.Category != null ? r.Category.Name : string.Empty,
                    Description     = r.Description,
                    Status          = r.Status.ToString(),
                    Latitude        = r.Latitude,
                    Longitude       = r.Longitude,
                    AssignedByName  = r.AssignedByAuthority != null ? r.AssignedByAuthority.Name : "Not assigned yet",
                    AssignedToName  = r.AssignedWorker  != null ? r.AssignedWorker.Name  : "Not assigned yet",
                    UpdatedAt       = r.UpdatedAt,
                    BlockageReason  = r.RejectionReason,
                    PhotoUrls       = r.ReportPics.Select(p => p.PicUrl).ToList()
                })
                .ToListAsync();

            return reports;
        }

        public async Task<Report?> GetUserReportByIdAsync(string userId, int reportId)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.SubmittedById == userId);
        }

        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
}
