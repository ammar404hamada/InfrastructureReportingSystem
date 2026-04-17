using InfraReportingSystem.Shared.DTOs.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Worker
{
    public interface IWorkerTasksService
    {
        Task<IEnumerable<WorkerTaskDto>> GetMyTasksAsync(string workerId, string? searchTerm);
    }
}
