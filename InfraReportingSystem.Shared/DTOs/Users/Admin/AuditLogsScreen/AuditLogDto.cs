using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.UserServices.Admin.AuditLogsScreen
{
    public class AuditLogDto
    {
        public DateTime Timestamp { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string ActorEmail { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
    }

}
