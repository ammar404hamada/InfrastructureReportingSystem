using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Map;
using Microsoft.EntityFrameworkCore;


namespace InfraReportingSystem.Persistence.Repositories.Authority.Map
{
    public class AuthorityMapRepository : IAuthorityMapRepository
    {
        private static readonly HashSet<ReportStatus> VisibleStatuses = new()
        {
            ReportStatus.Submitted,   // included as requested
            ReportStatus.Resolved,
            ReportStatus.Blocked,
            ReportStatus.Assigned,
            ReportStatus.InProgress,
        };

        private readonly AppDbContext _context;

        public AuthorityMapRepository(AppDbContext context)
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
