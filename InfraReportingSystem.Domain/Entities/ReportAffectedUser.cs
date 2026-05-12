using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities
{
    public class ReportAffectedUser
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
