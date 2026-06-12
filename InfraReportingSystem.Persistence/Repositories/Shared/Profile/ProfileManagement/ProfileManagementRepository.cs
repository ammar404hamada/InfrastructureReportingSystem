using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile.ProfileManagement;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Shared.Profile.ProfileManagement
{
    public class ProfileManagementRepository : IProfileManagementRepository
    {
        private readonly UserManager<User> _userManager;

        public ProfileManagementRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
