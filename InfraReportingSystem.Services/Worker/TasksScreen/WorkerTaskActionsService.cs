using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksScreen;
using InfraReportingSystem.Shared.DTOs.Worker.TasksScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Worker.TasksScreen
{
    public class WorkerTaskActionsService : IWorkerTaskActionsService
    {
        private readonly IWorkerTaskActionsRepository _actionsRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public WorkerTaskActionsService(
            IWorkerTaskActionsRepository actionsRepository,
            IAuditLogRepository auditLogRepository)
        {
            _actionsRepository = actionsRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<string> AcceptTaskAsync(int reportId, string workerId)
        {
            var report = await _actionsRepository.GetByIdAsync(reportId);
            if (report is null)
                return "Report not found.";

            if (report.AssignedWorkerId != workerId)
                return "You are not assigned to this task.";

            if (report.Status != ReportStatus.Assigned)
                return "This task cannot be accepted in its current status.";

            var hasActiveTask = await _actionsRepository.HasActiveTaskAsync(workerId);
            if (hasActiveTask)
                return "You already have an active task.";

            report.Status = ReportStatus.InProgress;
            await _actionsRepository.UpdateAsync(report);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = workerId,
                ActionType = AuditActionType.WorkerAcceptedTask,
                EntityName = "Report",
                EntityId = reportId.ToString(),
                Details = $"Worker {workerId} accepted task for report {reportId}."
            });

            return "Task accepted successfully.";
        }

        public async Task<string> RejectTaskAsync(int reportId, string workerId, RejectTaskDto dto)
        {
            var report = await _actionsRepository.GetByIdAsync(reportId);
            if (report is null)
                return "Report not found.";

            if (report.AssignedWorkerId != workerId)
                return "You are not assigned to this task.";

            if (report.Status != ReportStatus.Assigned)
                return "This task cannot be rejected in its current status.";

            report.Status = ReportStatus.WorkerRejected;
            report.RejectionReason = dto.Reason;
            report.AssignedWorkerId = null;
            report.AssignedByAuthorityId = null;

            await _actionsRepository.UpdateAsync(report);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = workerId,
                ActionType = AuditActionType.WorkerRejectedTask,
                EntityName = "Report",
                EntityId = reportId.ToString(),
                Details = $"Worker {workerId} rejected task for report {reportId}. Reason: {dto.Reason}"
            });

            return "Task rejected successfully.";
        }
    }
}
