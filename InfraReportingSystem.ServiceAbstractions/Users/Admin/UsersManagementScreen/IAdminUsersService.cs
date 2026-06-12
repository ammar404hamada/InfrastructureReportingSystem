using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Users.Admin.UsersManagementScreen
{
    public interface IAdminUsersService
    {
        Task<PaginatedResult<AdminUserDto>> GetUsersAsync(
            string? searchTerm,
            string? role,
            string? status,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
    }
}
