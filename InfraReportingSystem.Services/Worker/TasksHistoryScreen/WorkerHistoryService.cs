using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksHistoryScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Worker.TasksHistoryScreen;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Worker.TasksHistoryScreen
{
    public class WorkerHistoryService : IWorkerHistoryService
    {
        private readonly IWorkerHistoryRepository _repository;
        private readonly ILogger<WorkerHistoryService> _logger;

        private static readonly HashSet<ReportStatus> AllowedStatuses = new()
    {
        ReportStatus.Blocked,
        ReportStatus.Resolved,
        ReportStatus.Rejected,
        ReportStatus.FixRejected
    };

        public WorkerHistoryService(IWorkerHistoryRepository repository, ILogger<WorkerHistoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PaginatedResult<HistoryTaskDto>> GetHistoryAsync(
            string workerId,
            string? searchTerm,
            string? statusFilter,
            int pageNumber,
            int pageSize)
        {
            List<ReportStatus>? statusFilters = null;
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                var statusNames = statusFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToList();
                statusFilters = new List<ReportStatus>();
                foreach (var name in statusNames)
                {
                    if (Enum.TryParse<ReportStatus>(name, true, out var status) && AllowedStatuses.Contains(status))
                        statusFilters.Add(status);
                }
                if (statusFilters.Count == 0)
                {
                    return new PaginatedResult<HistoryTaskDto>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Items = new List<HistoryTaskDto>(),
                        Message = "No tasks found with the specified status."
                    };
                }
            }
            else
            {
                statusFilters = AllowedStatuses.ToList();
            }

            var (reports, totalCount) = await _repository.GetHistoryAsync(workerId, searchTerm, statusFilters, pageNumber, pageSize);
            var dtos = reports.Select(MapToHistoryDto).ToList();

            if (totalCount == 0)
                _logger.LogInformation("Worker {WorkerId} has no history tasks matching criteria.", workerId);

            return new PaginatedResult<HistoryTaskDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = dtos,
                Message = totalCount == 0 ? "No history found. Try adjusting your search or filter." : string.Empty
            };
        }

        private static HistoryTaskDto MapToHistoryDto(Report report)
        {
            return new HistoryTaskDto
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
                MapUrl = $"https://www.google.com/maps?q={report.Latitude},{report.Longitude}",
                RejectionReason = report.RejectionReason
            };
        }
    }
}
