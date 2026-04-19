using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Worker.TasksScreen;

namespace InfraReportingSystem.ServiceAbstractions.Worker;

public interface IWorkerTasksService
{
    Task<PaginatedResult<WorkerTaskDto>> GetMyTasksAsync(
        string workerId,
        string? searchTerm,
        int pageNumber,
        int pageSize);
}