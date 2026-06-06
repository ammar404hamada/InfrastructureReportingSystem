using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Shared.Auth { 

    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Code { get; set; }
        public UserDto? User { get; set; }
    }
}
