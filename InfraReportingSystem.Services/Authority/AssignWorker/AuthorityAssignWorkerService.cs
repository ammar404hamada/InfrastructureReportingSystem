using System;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.AssignWorker;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Authority.AssignWorker
{
    public class AuthorityAssignWorkerService : IAuthorityAssignWorkerService
    {
        private readonly IAuthorityAssignWorkerRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AuthorityAssignWorkerService> _logger;

        public AuthorityAssignWorkerService(
            IAuthorityAssignWorkerRepository repository,
            IAuditLogRepository auditLogRepository,
            IEmailService emailService,
            UserManager<User> userManager,
            ILogger<AuthorityAssignWorkerService> logger)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> AssignWorkerAsync(
            int reportId,
            string workerId,
            string authorityId)
        {
            var report = await _repository.GetByIdAsync(reportId);
            if (report is null)
            {
                _logger.LogWarning("AssignWorker: Report {ReportId} not found.", reportId);
                return (false, "Report not found.");
            }

            if (report.Status != ReportStatus.Submitted)
            {
                _logger.LogWarning(
                    "AssignWorker: Report {ReportId} has status '{Status}' — must be Submitted.",
                    reportId, report.Status);
                return (false,
                    $"Report cannot be assigned. Current status is '{report.Status}'. " +
                    "Only Submitted reports can be assigned.");
            }

            var worker = await _userManager.FindByIdAsync(workerId);
            if (worker is null)
            {
                _logger.LogWarning("AssignWorker: Worker {WorkerId} not found.", workerId);
                return (false, "Worker not found.");
            }

            if (worker.Status != UserStatus.Active)
            {
                _logger.LogWarning(
                    "AssignWorker: Worker {WorkerId} has status '{Status}' — must be Active.",
                    workerId, worker.Status);
                return (false, "Worker is not active and cannot be assigned.");
            }

            var roles = await _userManager.GetRolesAsync(worker);
            if (!roles.Contains("Worker"))
            {
                _logger.LogWarning(
                    "AssignWorker: User {WorkerId} does not have the Worker role.", workerId);
                return (false, "The specified user is not a worker.");
            }

            if (await _repository.HasActiveTaskAsync(workerId))
            {
                _logger.LogWarning(
                    "AssignWorker: Worker {WorkerId} already has an active task in progress.", workerId);
                return (false, "Worker already has an active task in progress.");
            }

            report.AssignedWorkerId = worker.Id;
            report.AssignedByAuthorityId = authorityId;
            report.Status = ReportStatus.Assigned;
            report.AssignedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = authorityId,
                ActionType = AuditActionType.ReportAssigned,
                EntityName = "Report",
                EntityId = reportId.ToString(),
                Details = $"Authority {authorityId} assigned report {reportId} " +
                          $"to worker {worker.Id} ({worker.Name}).",
                Timestamp = DateTime.UtcNow
            });

            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Report {ReportId} assigned to worker {WorkerId} by authority {AuthorityId}.",
                reportId, workerId, authorityId);

            await SendAssignmentEmailAsync(worker, report);

            return (true, "Worker assigned successfully.");
        }

        private async Task SendAssignmentEmailAsync(User worker, Report report)
        {
            try
            {
                var body = $@"
                    <h2>New Task Assigned</h2>
                    <p>Hi {worker.Name},</p>
                    <p>You have been assigned a new infrastructure report.</p>
                    <p><strong>Report ID:</strong> {report.Id}</p>
                    <p><strong>Description:</strong> {report.Description}</p>
                    <p><strong>Location:</strong> {report.Latitude:F4}, {report.Longitude:F4}</p>
                    <p>Please log in to the system to view and accept your task.</p>
                ";

                await _emailService.SendEmailAsync(
                    worker.Email!,
                    "New Task Assigned — Infrastructure Reporting System",
                    body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send assignment email to worker {WorkerId}.", worker.Id);
            }
        }
    }
}