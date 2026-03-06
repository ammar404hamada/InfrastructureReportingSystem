using InfraReportingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InfraReportingSystem.Domain.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = null!;

        public string? Pic_url { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Inactive;


        public ICollection<Report> SubmittedReports { get; set; } = new List<Report>();
    }
}