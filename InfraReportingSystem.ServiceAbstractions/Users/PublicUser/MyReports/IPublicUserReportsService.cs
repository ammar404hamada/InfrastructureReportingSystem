using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports
{
    public interface IPublicUserReportsService
    {
        Task<PaginatedResult<PublicUserReportsDto>> GetReportsByUserIdAsync(
            string userId,
            PublicUserReportsFilterDto filterDto);
        Task<FixConfirmationResponseDto> UpdateFixConfirmationStatusAsync(
            string userId,
            int reportId,
            ReportStatus newStatus);
    }
}
