using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Authority.IncomingReports;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Persistence.Repositories.Authority.IncomingReports
{
    public class AuthorityIncomingReportsRepository : IAuthorityIncomingReportsRepository
    {
        private readonly AppDbContext _context;

        public AuthorityIncomingReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Report> Reports, int TotalCount)> GetIncomingReportsAsync(
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Reports
                .AsNoTracking()
                .Include(r => r.SubmittedBy)
                .Include(r => r.ReportPics)
                .Where(r => r.Status == ReportStatus.Submitted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var searchTerm = $"%{term}%";

                query = query.Where(r =>
                    EF.Functions.Like(r.Description, searchTerm) ||
                    EF.Functions.Like($"{r.Latitude}, {r.Longitude}", searchTerm) ||
                    EF.Functions.Like(r.SubmittedBy.Name ?? string.Empty, searchTerm) ||
                    EF.Functions.Like(r.SubmittedBy.Email ?? string.Empty, searchTerm));
            }

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return (Enumerable.Empty<Report>(), 0);

            var isAsc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "title" => isAsc
                    ? query.OrderBy(r => r.Description)
                    : query.OrderByDescending(r => r.Description),

                "reporter" => isAsc
                    ? query.OrderBy(r => r.SubmittedBy.Name)
                    : query.OrderByDescending(r => r.SubmittedBy.Name),

                _ => isAsc
                    ? query.OrderBy(r => r.UploadedAt)
                    : query.OrderByDescending(r => r.UploadedAt)
            };

            var reports = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reports, totalCount);
        }
    }
}
