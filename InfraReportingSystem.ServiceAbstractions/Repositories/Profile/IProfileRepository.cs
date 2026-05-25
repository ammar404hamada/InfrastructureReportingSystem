using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Profile
{
    public interface IProfileRepository
    {
        Task<(User? User, IList<string> Roles)> GetUserProfileAsync(string userId);
    }
}
