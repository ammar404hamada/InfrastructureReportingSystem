using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.Services.Users.PublicUser.MyReports
{
    public class PublicUserReportsService : IPublicUserReportsService
    {
        private readonly IPublicUserReportsRepository _repository;

        public PublicUserReportsService(IPublicUserReportsRepository repository)
        {
            _repository = repository;
        }

        //public async Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId)
        //{
        //    return await _repository.GetReportsByUserIdAsync(userId);
        //}

        public async Task<FixConfirmationResponseDto> UpdateFixConfirmationStatusAsync(
            string userId,
            int reportId,
            ReportStatus newStatus)
        {
            var report = await _repository.GetUserReportByIdAsync(userId, reportId);

            if (report is null)
                return Fail("Report not found.");

            if (report.Status != ReportStatus.PendingConfirmation)
                return Fail("Only reports waiting for confirmation can be updated.");

            report.Status = newStatus;
            report.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(report);

            var message = newStatus == ReportStatus.Resolved
                ? "Fix confirmed successfully."
                : "Fix rejected successfully.";

            return new FixConfirmationResponseDto
            {
                Success = true,
                Message = message
            };
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
