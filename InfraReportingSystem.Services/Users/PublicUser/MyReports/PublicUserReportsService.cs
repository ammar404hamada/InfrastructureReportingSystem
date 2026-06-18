using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;
using InfrastructureReportingSystem.Shared.EmailTemplate;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Users.PublicUser.MyReports
{
    public class PublicUserReportsService : IPublicUserReportsService
    {
        private readonly IPublicUserReportsRepository _repository;
        private readonly IEmailService _emailService;
        private readonly ILogger<PublicUserReportsService> _logger;

        public PublicUserReportsService(
            IPublicUserReportsRepository repository,
            IEmailService emailService,
            ILogger<PublicUserReportsService> logger)
        {
            _repository = repository;
            _emailService = emailService;
            _logger = logger;
        }

        //public async Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId)
        //{
        //    return await _repository.GetReportsByUserIdAsync(userId);
        //}

        public async Task<FixConfirmationResponseDto> UpdateFixConfirmationStatusAsync(
            string userId,
            int reportId,
            ReportStatus newStatus,
            string? reason = null)
        {
            var report = await _repository.GetUserReportByIdAsync(userId, reportId);

            if (report is null)
                return Fail("Report not found.");

            if (report.Status != ReportStatus.PendingConfirmation)
                return Fail("Only reports waiting for confirmation can be updated.");

            report.Status = newStatus;
            report.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(report);

            _ = NotifyWorkerAsync(report, newStatus, reason);

            var message = newStatus == ReportStatus.Resolved
                ? "Fix confirmed successfully."
                : "Fix rejected successfully.";

            return new FixConfirmationResponseDto
            {
                Success = true,
                Message = message
            };
        }

        private async Task NotifyWorkerAsync(Report report, ReportStatus newStatus, string? reason)
        {
            try
            {
                var worker = report.AssignedWorker;

                if (worker is null || string.IsNullOrWhiteSpace(worker.Email))
                {
                    _logger.LogWarning("Cannot send fix confirmation email for report {ReportId}: worker or email is missing.", report.Id);
                    return;
                }

                var isConfirmed = newStatus == ReportStatus.Resolved;
                var statusLabel = isConfirmed ? "Resolved" : "Fix Rejected";
                var messageText = isConfirmed
                    ? $"The user has confirmed the fix for report #{report.Id}."
                    : $"The user has rejected the fix for report #{report.Id}.";
                var timestamp = DateTime.UtcNow.ToString("f");

                var body = EmailTemplates.FixConfirmationEmailTemplate(
                    WorkerName: worker.Name,
                    WorkerEmail: worker.Email,
                    ReportId: report.Id.ToString(),
                    Description: report.Description,
                    CategoryName: report.Category?.Name ?? "Unknown",
                    StatusLabel: statusLabel,
                    Message: messageText,
                    Reason: isConfirmed ? null : reason,
                    Timestamp: timestamp
                );

                var subject = $"Report #{report.Id} – Update on Your Fix";

                await _emailService.SendEmailAsync(worker.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send fix confirmation email for report {ReportId}.", report.Id);
            }
        }

        public async Task<PaginatedResult<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId, PublicUserReportsFilterDto filter)
        {
            filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
            filter.PageSize = filter.PageSize > 50 ? 50 : filter.PageSize;

            var (items, totalCount) = await _repository.GetReportsByUserIdAsync(
            userId,
            filter.PageNumber,
            filter.PageSize);

            return new PaginatedResult<PublicUserReportsDto>
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Items = items,
                Message = totalCount == 0 ? "No reports found." : string.Empty
            };
        }

        private static FixConfirmationResponseDto Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
