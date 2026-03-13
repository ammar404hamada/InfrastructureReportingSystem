using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Auth { 

    public class LoginResponseDto
    {
        public string Message { get; set; }
        public UserDto User { get; set; }
        public string Token { get; set; }
    }
}