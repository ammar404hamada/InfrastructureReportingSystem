namespace InfraReportingSystem.Shared.DTOs.Profile
{
    public class UserProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileDto? Profile { get; set; }
    }
}
