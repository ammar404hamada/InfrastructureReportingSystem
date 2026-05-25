using System;
using System.Collections.Generic;

namespace InfraReportingSystem.Shared.DTOs.Profile
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public string? Specialization { get; set; }
    }
}
