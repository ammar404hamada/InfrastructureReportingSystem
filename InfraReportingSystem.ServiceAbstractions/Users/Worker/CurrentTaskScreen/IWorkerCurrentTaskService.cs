using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen
{
    public interface IWorkerCurrentTaskService
    {
        Task<PaginatedResult<CurrentTaskDto>> GetCurrentTaskAsync(string workerId);
    }
}
