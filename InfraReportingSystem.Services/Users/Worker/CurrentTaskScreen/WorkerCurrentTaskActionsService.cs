using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using InfrastructureReportingSystem.Shared.EmailTemplate;
using Microsoft.Extensions.Logging;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<WorkerCurrentTaskActionsService> _logger;

        public WorkerCurrentTaskActionsService(
            IWorkerCurrentTaskActionsRepository repository,
            IAuditLogRepository auditLogRepository,
            IEmailService emailService,
            ILogger<WorkerCurrentTaskActionsService> logger)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
            _emailService = emailService;
            _logger = logger;
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

            _ = NotifyUserAsync(report, "Pending Confirmation", null);

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

            _ = NotifyUserAsync(report, "Blocked", reason);

            return "Task marked as blocked. Authority will review.";
        }

        private async Task NotifyUserAsync(Report report, string statusLabel, string? reason)
        {
            try
            {
                var submitter = report.SubmittedBy;

                if (submitter is null || string.IsNullOrWhiteSpace(submitter.Email))
                {
                    _logger.LogWarning("Cannot send status update email for report {ReportId}: submitter or email is missing.", report.Id);
                    return;
                }

                var timestamp = DateTime.UtcNow.ToString("f");
                var workerName = report.AssignedWorker?.Name ?? "Unknown Worker";

                var body = EmailTemplates.WorkerActionEmailTemplate(
                    UserName: submitter.Name,
                    UserEmail: submitter.Email,
                    ReportId: report.Id.ToString(),
                    Description: report.Description,
                    CategoryName: report.Category?.Name ?? "Unknown",
                    WorkerName: workerName,
                    StatusLabel: statusLabel,
                    Reason: reason,
                    Timestamp: timestamp
                );

                var subject = $"Report #{report.Id} – Status Updated";

                await _emailService.SendEmailAsync(submitter.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send status update email for report {ReportId}.", report.Id);
            }
        }
    }
}
