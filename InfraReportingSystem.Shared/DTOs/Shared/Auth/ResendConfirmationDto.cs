using System.ComponentModel.DataAnnotations;

namespace InfraReportingSystem.Shared.DTOs.Shared.Auth {

    public class ResendConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
