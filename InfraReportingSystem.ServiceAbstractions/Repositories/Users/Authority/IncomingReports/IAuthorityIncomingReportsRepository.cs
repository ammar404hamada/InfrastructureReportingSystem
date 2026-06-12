using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.IncomingReports
{
    public interface IAuthorityIncomingReportsRepository
    {
        Task<(IEnumerable<Report> Reports, int TotalCount)> GetIncomingReportsAsync(
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
    }
}

