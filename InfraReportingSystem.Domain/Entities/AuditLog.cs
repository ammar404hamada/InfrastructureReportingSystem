using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities
{
    public class AuditLog
    {

        public int Id { get; set; }

        // The user who performed the action
        // Nullable because failed login attempts may not belong to a valid user
        public string? UserId { get; set; }
        public User? User { get; set; }

        // The type of action performed in the system
        public AuditActionType ActionType { get; set; }

        // The entity affected by the action (e.g., "Report", "User")
        public string? EntityName { get; set; }

        // The ID of the affected entity
        public string? EntityId { get; set; }

        // Detailed description of the action
        public string Details { get; set; } = null!;

        // Timestamp of when the action occurred
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
