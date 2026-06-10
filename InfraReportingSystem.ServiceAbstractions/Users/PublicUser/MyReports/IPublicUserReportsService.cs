using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports
{
    public interface IPublicUserReportsService
    {
        Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId);
        Task<FixConfirmationResponseDto> UpdateFixConfirmationStatusAsync(
            string userId,
            int reportId,
            ReportStatus newStatus);
    }
}
