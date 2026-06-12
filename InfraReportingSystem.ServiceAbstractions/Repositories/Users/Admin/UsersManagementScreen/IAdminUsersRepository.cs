using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen
{
    public interface IAdminUsersRepository
    {
        Task<(IEnumerable<User> Items, int TotalCount)> GetUsersAsync(
            string? searchTerm,
            string? roleFilter,
            UserStatus? statusFilter,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
    }
}
