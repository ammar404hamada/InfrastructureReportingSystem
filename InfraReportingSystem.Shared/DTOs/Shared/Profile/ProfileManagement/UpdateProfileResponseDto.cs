using InfraReportingSystem.Shared.DTOs.Shared;

namespace InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement
{
    public class UpdateProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProfileDataDto? Data { get; set; }
    }
}
