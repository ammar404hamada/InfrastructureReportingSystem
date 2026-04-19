using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Worker
{
    public interface IWorkerTasksRepository
    {
        Task<(IEnumerable<Report> Items, int TotalCount)> GetMyTasksAsync(
            string workerId,
            string? searchTerm,
            int pageNumber,
            int pageSize);
    }
}
