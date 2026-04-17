using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories
{
    public interface IWorkerTasksRepository
    {
        Task<IEnumerable<Report>> GetMyTasksAsync(string workerId, string? searchTerm);
    }
}
