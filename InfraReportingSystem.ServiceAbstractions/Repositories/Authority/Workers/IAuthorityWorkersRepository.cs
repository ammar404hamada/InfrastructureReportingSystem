using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers
{
    public interface IAuthorityWorkersRepository
    {
        // "Worker" is the domain entity — not "Workers"
        Task<(IEnumerable<User> Workers, int TotalCount)> GetWorkersAsync(
     string? search,
     int pageNumber,
     int pageSize);

    }
}
