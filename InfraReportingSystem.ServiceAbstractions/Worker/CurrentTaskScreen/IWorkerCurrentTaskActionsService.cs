using InfraReportingSystem.Shared.DTOs.Worker.CurrentTaskScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Worker.CurrentTaskScreen
{
    public interface IWorkerCurrentTaskActionsService
    {
        Task<string> MarkAsFixedAsync(string workerId, MarkFixedDto dto);
        Task<string> MarkAsBlockedAsync(string workerId, MarkBlockedDto dto);
    }
}
