using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Profile;

namespace InfraReportingSystem.ServiceAbstractions.Profile
{
    public interface IProfileService
    {
        Task<UserProfileResponseDto> GetProfileAsync(string userId);
    }
}
