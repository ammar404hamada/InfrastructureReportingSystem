using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksScreen
{
    public interface IWorkerTaskActionsService
    {
        Task<string> AcceptTaskAsync(int reportId, string workerId);
        Task<string> RejectTaskAsync(int reportId, string workerId, RejectTaskDto dto);
    }
}
