using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.UsersManagementScreen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Admin.UsersManagementScreen
{
    public class AdminUsersRepository : IAdminUsersRepository
    {
        private readonly AppDbContext _context;

        public AdminUsersRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetUsersAsync(
            string? searchTerm,
            string? roleFilter,
            UserStatus? statusFilter,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            var adminUserIds =
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == "Admin"
                select ur.UserId;

            IQueryable<User> query = _context.Users
                .AsNoTracking()
                .Where(u => u.Status != UserStatus.Deleted)
                .Where(u => !adminUserIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = roleFilter switch
                {
                    "Worker" => query.OfType<InfraReportingSystem.Domain.Entities.Worker>(),
                    "Authority" => query.OfType<InfraReportingSystem.Domain.Entities.Authority>(),
                    "PublicUser" => query.Where(u =>
                        EF.Property<string>(u, "UserType") == "User"),
                    _ => query
                };
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.Name, $"%{term}%") ||
                    EF.Functions.Like(u.PhoneNumber ?? string.Empty, $"%{term}%") ||
                    EF.Functions.Like(u.Email ?? string.Empty, $"%{term}%"));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(u => u.Status == statusFilter.Value);
            }

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return (Array.Empty<User>(), 0);
            }

            var isAsc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<User> orderedQuery = sortBy?.Trim().ToLowerInvariant() switch
            {
                "name" => isAsc
                    ? query.OrderBy(u => u.Name)
                    : query.OrderByDescending(u => u.Name),

                "email" => isAsc
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),

                "joindate" => isAsc
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt),

                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
