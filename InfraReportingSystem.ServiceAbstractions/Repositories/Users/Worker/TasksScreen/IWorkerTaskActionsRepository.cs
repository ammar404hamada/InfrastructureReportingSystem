using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen
{
    public interface IWorkerTaskActionsRepository
    {
        Task<Report?> GetByIdAsync(int reportId);
        Task<bool> HasActiveTaskAsync(string workerId);
        Task UpdateAsync(Report report);
    }
}
