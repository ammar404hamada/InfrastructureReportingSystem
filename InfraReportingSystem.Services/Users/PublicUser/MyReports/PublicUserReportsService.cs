using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
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

        public async Task<IEnumerable<PublicUserReportsDto>> GetReportsByUserIdAsync(string userId)
        {
            return await _repository.GetReportsByUserIdAsync(userId);
        }
    }
}
