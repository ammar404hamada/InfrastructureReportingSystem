using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Shared.Auth { 

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
