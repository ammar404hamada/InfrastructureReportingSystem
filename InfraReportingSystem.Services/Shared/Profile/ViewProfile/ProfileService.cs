using System.Linq;
using System.Threading.Tasks;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile;
using InfraReportingSystem.ServiceAbstractions.Shared.Profile;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ViewProfile;

namespace InfraReportingSystem.Services.Shared.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
        {
            var (user, roles) = await _profileRepository.GetUserProfileAsync(userId);
            
            if (user == null)
            {
                return new UserProfileResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var specialization = ((user as Domain.Entities.Worker))?.Specialization;

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                Specialization = specialization
            };

            return new UserProfileResponseDto
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Profile = profileDto
            };
        }
    }
}
