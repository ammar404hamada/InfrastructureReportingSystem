using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.MarkerPopup;
using Microsoft.EntityFrameworkCore;


namespace InfraReportingSystem.Persistence.Repositories.Users.Authority.MarkerPopup
{
    public class AuthorityMarkerPopupRepository : IAuthorityMarkerPopupRepository
    {
        private static readonly HashSet<ReportStatus> VisibleStatuses = new()
        {
            ReportStatus.Submitted,
            ReportStatus.Resolved,
            ReportStatus.Blocked,
            ReportStatus.Assigned,
            ReportStatus.InProgress,
        };

        private readonly AppDbContext _context;

        public AuthorityMarkerPopupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetReportAsync(int reportId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.ReportPics)
                .Include(r => r.SubmittedBy)
                .FirstOrDefaultAsync(r =>
                    r.Id == reportId &&
                    VisibleStatuses.Contains(r.Status));
        }
    }
}
