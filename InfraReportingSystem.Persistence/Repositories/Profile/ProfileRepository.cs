using System.Collections.Generic;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Profile;
using Microsoft.AspNetCore.Identity;

namespace InfraReportingSystem.Persistence.Repositories.Profile
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly UserManager<User> _userManager;

        public ProfileRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(User? User, IList<string> Roles)> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return (null, new List<string>());
            }

            var roles = await _userManager.GetRolesAsync(user);
            return (user, roles);
        }
    }
}
