using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Auth { 

    public class RegisterResponseDto
    {
        public string Message { get; set; }
        public UserDto User { get; set; }
    }
}