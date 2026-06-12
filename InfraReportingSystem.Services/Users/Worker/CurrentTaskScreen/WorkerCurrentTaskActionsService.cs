using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Users.Worker.CurrentTaskScreen
{
    public class WorkerCurrentTaskActionsService : IWorkerCurrentTaskActionsService
    {
        private readonly IWorkerCurrentTaskActionsRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;

        public WorkerCurrentTaskActionsService(
            IWorkerCurrentTaskActionsRepository repository,
            IAuditLogRepository auditLogRepository)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<string> MarkAsFixedAsync(string workerId, MarkFixedDto dto)
        {
            var report = await _repository.GetCurrentTaskAsync(workerId);

            if (report is null)
                return "No active task found to mark as fixed.";

            var comment = string.IsNullOrWhiteSpace(dto.Comment)
                ? "None"
                : dto.Comment.Trim();

            report.Status = ReportStatus.PendingConfirmation;

            await _repository.UpdateAsync(report);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = workerId,
                ActionType = AuditActionType.WorkerMarkedTaskAsFixed,
                EntityName = "Report",
                EntityId = report.Id.ToString(),
                Details = $"Status changed from InProgress to PendingConfirmation. Comment: {comment}"
            });

            return "Task marked as fixed. Waiting for user confirmation.";
        }

        public async Task<string> MarkAsBlockedAsync(string workerId, MarkBlockedDto dto)
        {
            var reason = dto.Reason?.Trim();

            if (string.IsNullOrWhiteSpace(reason))
                return "Reason is required when blocking a task.";

            var report = await _repository.GetCurrentTaskAsync(workerId);

            if (report is null)
                return "No active task found to mark as blocked.";

            report.Status = ReportStatus.Blocked;
            report.RejectionReason = reason;

            await _repository.UpdateAsync(report);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = workerId,
                ActionType = AuditActionType.ReportBlocked,
                EntityName = "Report",
                EntityId = report.Id.ToString(),
                Details = $"Status changed from InProgress to Blocked. Reason: {reason}"
            });

            return "Task marked as blocked. Authority will review.";
        }
    }
}
