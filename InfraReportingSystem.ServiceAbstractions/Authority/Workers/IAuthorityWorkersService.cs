using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.Common;

using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers
{
    public interface IAuthorityWorkersService
    {
        Task<PaginatedResult<WorkerListDto>> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize);
    }
}