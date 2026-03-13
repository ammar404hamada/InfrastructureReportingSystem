using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Auth { 
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(16)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Password does not match")]
        public string RePassword { get; set; }
        

    }
}