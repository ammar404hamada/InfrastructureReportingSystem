using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksScreen;

namespace InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksScreen;

public interface IWorkerTasksService
{
    Task<PaginatedResult<WorkerTaskDto>> GetMyTasksAsync(
        string workerId,
        string? searchTerm,
        int pageNumber,
        int pageSize);
}