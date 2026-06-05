using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.Workers;
using Microsoft.EntityFrameworkCore;

using WorkerEntity = InfraReportingSystem.Domain.Entities.Worker;

namespace InfraReportingSystem.Persistence.Repositories.Users.Authority.Workers
{
    public class AuthorityWorkersRepository : IAuthorityWorkersRepository
    {
        private readonly AppDbContext _context;

        public AuthorityWorkersRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<(User Worker, int ActiveTaskCount)> Workers, int TotalCount)> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize)
        {
            var workerUserIds =
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == "Worker"
                select ur.UserId;

            IQueryable<User> query = _context.Workers
                .AsNoTracking()
                .Where(w => workerUserIds.Contains(w.Id) && w.Status == UserStatus.Active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(w =>
                    EF.Functions.Like(w.Name, $"%{term}%") ||
                    EF.Functions.Like(w.Email ?? string.Empty, $"%{term}%") ||
                    EF.Functions.Like(w.PhoneNumber ?? string.Empty, $"%{term}%"));
            }

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return (Enumerable.Empty<(User, int)>(), 0);

            var activeTaskCounts = await _context.Reports
                .AsNoTracking()
                .Where(r => r.Status == ReportStatus.InProgress && r.AssignedWorkerId != null)
                .GroupBy(r => r.AssignedWorkerId!)
                .Select(g => new { WorkerId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.WorkerId, x => x.Count);

            var workers = await query
                .OrderBy(w => w.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = workers
                .Select(w => (w, activeTaskCounts.GetValueOrDefault(w.Id, 0)));

            return (result, totalCount);
        }
    }
}