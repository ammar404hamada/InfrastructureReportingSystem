using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports
{
    public interface IPublicUserReportsService
    {
        Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId);
    }
}
