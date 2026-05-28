using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MarkerPopup;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Persistence.Repositories.Users.PublicUser.MarkerPopup
{
    public class PublicMarkerPopupRepository : IPublicMarkerPopupRepository
    {
        private static readonly HashSet<ReportStatus> VisibleStatuses = new()
        {
            ReportStatus.Submitted,
            ReportStatus.Assigned,
            ReportStatus.InProgress,
            ReportStatus.Blocked,
            ReportStatus.PendingConfirmation,
            ReportStatus.Resolved
        };

        private readonly AppDbContext _context;

        public PublicMarkerPopupRepository(AppDbContext context)
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
                .Include(r => r.AssignedWorker)
                .FirstOrDefaultAsync(r => r.Id == reportId
                                       && VisibleStatuses.Contains(r.Status));
        }
    }
}
