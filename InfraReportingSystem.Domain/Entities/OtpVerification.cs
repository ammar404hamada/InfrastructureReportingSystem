using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities
{
    public class OtpVerification
    {

        public int Id { get; set; }

        public string OtpCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
        public OtpPurpose Purpose { get; set; }

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
