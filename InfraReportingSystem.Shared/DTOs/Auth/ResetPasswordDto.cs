using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Auth { 

    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
        [Required]
        [Compare("ConfirmPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}