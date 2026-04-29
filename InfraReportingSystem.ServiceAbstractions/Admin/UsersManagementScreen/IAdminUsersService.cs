using InfraReportingSystem.Shared.DTOs.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Admin.UsersManagementScreen
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
