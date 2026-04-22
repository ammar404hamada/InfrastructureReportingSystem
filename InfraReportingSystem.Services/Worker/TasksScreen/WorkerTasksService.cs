using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Worker.TasksScreen;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Worker.TasksScreen;

public class WorkerTasksService : IWorkerTasksService
{
    private readonly IWorkerTasksRepository _workerTasksRepository;
    private readonly ILogger<WorkerTasksService> _logger;

    public WorkerTasksService(
        IWorkerTasksRepository workerTasksRepository,
        ILogger<WorkerTasksService> logger)
    {
        _workerTasksRepository = workerTasksRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<WorkerTaskDto>> GetMyTasksAsync(
        string workerId,
        string? searchTerm,
        int pageNumber,
        int pageSize)
    {
        var (reports, totalCount) = await _workerTasksRepository.GetMyTasksAsync(workerId, searchTerm, pageNumber, pageSize);

        var dtos = reports.Select(MapToDto).ToList();

        if (totalCount == 0)
        {
            _logger.LogInformation(
                "Worker {WorkerId} has no assigned tasks. SearchTerm: {SearchTerm}",
                workerId,
                searchTerm ?? "null");
        }

        string message = totalCount == 0
            ? "No tasks found. Try adjusting your search or check back later."
            : string.Empty;

        return new PaginatedResult<WorkerTaskDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = dtos,
            Message = message
        };
    }

    private static WorkerTaskDto MapToDto(Report report)
    {
        return new WorkerTaskDto
        {
            Id = report.Id,
            Category = report.Category?.Name ?? string.Empty,
            Description = report.Description,
            Photos = report.ReportPics.Select(p => p.PicUrl).ToList(),
            Status = report.Status.ToString(),
            Latitude = report.Latitude,
            Longitude = report.Longitude,
            SubmittedByName = report.SubmittedBy?.Name ?? string.Empty,
            SubmittedAt = report.UploadedAt,
            AssignedByName = report.AssignedByAuthority?.Name ?? string.Empty,
            AssignedAt = report.AssignedAt,
            MapUrl = $"https://www.google.com/maps?q={report.Latitude},{report.Longitude}"
        };
    }
}