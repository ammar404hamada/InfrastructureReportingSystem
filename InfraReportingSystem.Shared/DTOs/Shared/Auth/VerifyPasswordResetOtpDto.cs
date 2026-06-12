using System.ComponentModel.DataAnnotations;

namespace InfraReportingSystem.Shared.DTOs.Shared.Auth
{
    public class VerifyPasswordResetOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;
    }
}
