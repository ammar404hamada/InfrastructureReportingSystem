using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Common;

using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.Workers;

namespace InfraReportingSystem.ServiceAbstractions.Users.Authority.Workers
{
    public interface IAuthorityWorkersService
    {
        Task<PaginatedResult<WorkerListDto>> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize);
    }
}