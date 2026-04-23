using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksHistoryScreen
{
    public interface IWorkerHistoryRepository
    {
        Task<(IEnumerable<Report> Items, int TotalCount)> GetHistoryAsync(
            string workerId,
            string? searchTerm,
            List<ReportStatus>? statusFilters,
            int pageNumber,
            int pageSize);
    }
}
