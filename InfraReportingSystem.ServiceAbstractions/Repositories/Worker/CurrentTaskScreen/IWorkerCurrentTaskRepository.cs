using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen
{
    public interface IWorkerCurrentTaskRepository
    {
        Task<Report?> GetCurrentTaskAsync(string workerId);
    }
}
