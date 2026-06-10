using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports
{
    public interface IPublicUserReportsRepository
    {
        Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId);
        Task<Report?> GetUserReportByIdAsync(string userId, int reportId);
        Task UpdateAsync(Report report);
    }
}
