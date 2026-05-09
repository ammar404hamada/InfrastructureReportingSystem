using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.Map;

namespace InfraReportingSystem.Persistence.Repositories.PublicUser.Map
{
    public class PublicMapRepository : IPublicMapRepository
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

        public PublicMapRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status)
        {
            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Where(r => VisibleStatuses.Contains(r.Status));

            if (status.HasValue)
            {
                if (!VisibleStatuses.Contains(status.Value))
                    return Enumerable.Empty<Report>();

                query = query.Where(r => r.Status == status.Value);
            }

            if (categoryId.HasValue)
                query = query.Where(r => r.CategoryId == categoryId.Value);

            return await query
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();
        }
    }
}
