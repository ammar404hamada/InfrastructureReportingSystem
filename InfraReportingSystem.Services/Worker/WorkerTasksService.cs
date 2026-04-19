using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker;
using InfraReportingSystem.ServiceAbstractions.Worker;
using InfraReportingSystem.Shared.DTOs.Worker;

namespace InfraReportingSystem.Services.Worker;

public class WorkerTasksService : IWorkerTasksService
{
    private readonly IWorkerTasksRepository _workerTasksRepository;

    public WorkerTasksService(IWorkerTasksRepository workerTasksRepository)
    {
        _workerTasksRepository = workerTasksRepository;
    }

    public async Task<IEnumerable<WorkerTaskDto>> GetMyTasksAsync(string workerId, string? searchTerm)
    {
        var reports = await _workerTasksRepository.GetMyTasksAsync(workerId, searchTerm);
        return reports.Select(MapToDto);
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