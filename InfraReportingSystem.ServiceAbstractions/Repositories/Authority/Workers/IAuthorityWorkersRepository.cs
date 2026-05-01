using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers
{
    public interface IAuthorityWorkersRepository
    {
        Task<(IEnumerable<Worker> Workers, int TotalCount)> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize);
    }
}
