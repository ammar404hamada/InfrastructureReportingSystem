namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen
{
    public class AdminUserProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminUserProfileDto? Profile { get; set; }
    }
}
