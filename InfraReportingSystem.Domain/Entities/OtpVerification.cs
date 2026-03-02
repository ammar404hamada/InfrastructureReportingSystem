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

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;


        // Foreign Key
        public int UserId { get; set; }

        // Navigation
        public User User { get; set; } = null!;

    }
}
