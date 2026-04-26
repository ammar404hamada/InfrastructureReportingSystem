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

        public string? ProfilePictureUrl { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Inactive;

        public DateTime CreatedAt { get; set; }

        public ICollection<Report> SubmittedReports { get; set; } = new List<Report>();

        public ICollection<OtpVerification> OtpVerifications { get; set; } = new List<OtpVerification>();

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}