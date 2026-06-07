using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports
{
    public interface IPublicUserReportsRepository
    {
        Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId);
    }
}
