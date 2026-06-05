using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using System.IO;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Shared.Profile.ProfileManagement
{
    public interface IProfileManagementService
    {
        Task<UpdateProfileResponseDto> UpdateProfileInfoAsync(string userId, UpdateProfileInfoDto dto);
        Task<UpdateProfileResponseDto> UpdateProfilePhotoAsync(string userId, Stream fileStream, string fileName);
    }
}
