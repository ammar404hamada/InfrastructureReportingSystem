using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ViewProfile;

namespace InfraReportingSystem.ServiceAbstractions.Shared.Profile
{
    public interface IProfileService
    {
        Task<UserProfileResponseDto> GetProfileAsync(string userId);
    }
}
