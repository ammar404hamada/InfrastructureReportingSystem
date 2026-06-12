using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksHistoryScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksHistoryScreen
{
    public interface IWorkerHistoryService
    {
        Task<PaginatedResult<HistoryTaskDto>> GetHistoryAsync(
            string workerId,
            string? searchTerm,
            string? statusFilter,
            int pageNumber,
            int pageSize);
    }
}
