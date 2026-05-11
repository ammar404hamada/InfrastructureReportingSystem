using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.PublicUser.AlsoSuffer;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.AlsoSuffer;
using Microsoft.EntityFrameworkCore;

namespace InfraReportingSystem.Services.PublicUser.AlsoSuffer
{
    public class PublicAlsoSufferService : IPublicAlsoSufferService
    {
        private static readonly HashSet<ReportStatus> AllowedStatuses = new()
        {
            ReportStatus.Submitted,
            ReportStatus.InProgress,
            ReportStatus.Blocked,
            ReportStatus.Resolved,
        };

        private readonly IPublicAlsoSufferRepository _repository;

        public PublicAlsoSufferService(IPublicAlsoSufferRepository repository)
        {
            _repository = repository;
        }

        public async Task<AlsoSufferResponseDto> ConfirmAlsoSufferAsync(int reportId, string userId)
        {
            var status = await _repository.GetReportStatusAsync(reportId);
            if (status is null)
                return NotFound("Report not found.");

            if (!AllowedStatuses.Contains(status.Value))
                return Fail("This report is no longer open for confirmations.");

            if (await _repository.AlreadyAffectedAsync(reportId, userId))
                return Fail("You already confirmed this issue.");

            var record = new ReportAffectedUser
            {
                ReportId = reportId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(record);

            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Fail("You already confirmed this issue.");
            }

            return new AlsoSufferResponseDto
            {
                Success = true,
                Message = "Your confirmation has been recorded."
            };
        }

        private static AlsoSufferResponseDto Fail(string message) =>
            new() { Success = false, Message = message };

        private static AlsoSufferResponseDto NotFound(string message) =>
            new() { Success = false, Message = message };
    }

}
