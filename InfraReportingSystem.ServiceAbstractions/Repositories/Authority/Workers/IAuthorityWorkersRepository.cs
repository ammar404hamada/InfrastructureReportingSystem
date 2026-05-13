using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers
{
    public interface IAuthorityWorkersRepository
    {
        Task<(IEnumerable<(User Worker, int ActiveTaskCount)> Workers, int TotalCount)> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize);
    }
}
