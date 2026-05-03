using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.AuditLogsScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Admin.AuditLogsScreen
{
    public class AdminAuditLogsRepository : IAdminAuditLogsRepository
    {
        private readonly AppDbContext _context;

        public AdminAuditLogsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<AuditLog> Logs, int TotalCount, Dictionary<string, string> UserRoles)> GetAuditLogsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            string? normalizedRole,
            AuditActionType? actionType)
        {
            IQueryable<AuditLog> query = _context.AuditLog
                .Include(x => x.User)
                .AsNoTracking();

            if (startDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.Timestamp <= end);
            }

            if (actionType.HasValue)
            {
                query = query.Where(x => x.ActionType == actionType.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.User != null &&
                    (
                        EF.Functions.Like(x.User.Name, $"%{search}%") ||
                        (x.User.Email != null && EF.Functions.Like(x.User.Email, $"%{search}%"))
                    ));
            }

            if (!string.IsNullOrWhiteSpace(normalizedRole))
            {
                if (normalizedRole == "system")
                {
                    query = query.Where(x => x.UserId == null);
                }
                else
                {
                    var roleId = await _context.Roles
                        .Where(r => r.NormalizedName == normalizedRole.ToUpperInvariant())
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrWhiteSpace(roleId))
                    {
                        return (new List<AuditLog>(), 0, new Dictionary<string, string>());
                    }

                    var userIdsInRole = _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId);

                    query = query.Where(x =>
                        x.UserId != null &&
                        userIdsInRole.Contains(x.UserId));
                }
            }

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return (new List<AuditLog>(), 0, new Dictionary<string, string>());
            }

            var logs = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = logs
                .Where(x => x.UserId != null)
                .Select(x => x.UserId!)
                .Distinct()
                .ToList();

            var userRoles = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Role = g.Select(x => x.Name).FirstOrDefault() ?? "PublicUser"
                })
                .ToDictionaryAsync(x => x.UserId, x => x.Role);

            return (logs, totalCount, userRoles);
        }
    }

}
