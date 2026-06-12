using InfraReportingSystem.Domain.Entities;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile.ProfileManagement
{
    public interface IProfileManagementRepository
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(User user);
    }
}
