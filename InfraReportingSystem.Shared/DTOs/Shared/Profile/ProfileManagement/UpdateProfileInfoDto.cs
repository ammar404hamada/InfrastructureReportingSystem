using System;
using System.ComponentModel.DataAnnotations;

namespace InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement
{
    public class UpdateProfileInfoDto
    {
        public string? Name { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
