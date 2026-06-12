using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ViewProfile;

namespace InfraReportingSystem.ServiceAbstractions.Profile
{
    public interface IProfileService
    {
        Task<UserProfileResponseDto> GetProfileAsync(string userId);
        Task<ChangePasswordResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
