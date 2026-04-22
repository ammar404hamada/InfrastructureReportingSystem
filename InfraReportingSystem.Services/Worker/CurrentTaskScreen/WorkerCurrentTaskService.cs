using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Worker.CurrentTaskScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Worker.CurrentTaskScreen
{
    public class WorkerCurrentTaskService : IWorkerCurrentTaskService
    {
        private readonly IWorkerCurrentTaskRepository _repository;

        public WorkerCurrentTaskService(IWorkerCurrentTaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedResult<CurrentTaskDto>> GetCurrentTaskAsync(string workerId)
        {
            var report = await _repository.GetCurrentTaskAsync(workerId);

            if (report is null)
            {
                return new PaginatedResult<CurrentTaskDto>
                {
                    PageNumber = 1,
                    PageSize = 1,
                    TotalCount = 0,
                    Items = new List<CurrentTaskDto>(),
                    Message = "No current task in progress"
                };
            }

            var dto = MapToDto(report);

            return new PaginatedResult<CurrentTaskDto>
            {
                PageNumber = 1,
                PageSize = 1,
                TotalCount = 1,
                Items = new List<CurrentTaskDto> { dto },
                Message = string.Empty
            };
        }

        private static CurrentTaskDto MapToDto(Report report)
        {
            return new CurrentTaskDto
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
}
