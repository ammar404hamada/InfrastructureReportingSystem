namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen
{
    public class AdminUserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime JoinDate { get; set; }
        public string? Specialization { get; set; }
    }
}
