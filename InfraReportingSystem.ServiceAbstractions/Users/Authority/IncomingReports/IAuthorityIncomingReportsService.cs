using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.IncomingReports;

namespace InfraReportingSystem.ServiceAbstractions.Users.Authority.IncomingReports
{
    public interface IAuthorityIncomingReportsService
    {
        Task<PaginatedResult<IncomingReportDto>> GetIncomingReportsAsync(
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
    }
}
