using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums
{
    public enum UserStatus
    {

        // Account created but not yet verified via OTP
        Inactive = 1,

        // OTP verification completed and the user can log in normally
        Active = 2,

        // Account temporarily locked due to multiple failed login attempts
        Locked = 3,

        // Account manually disabled by an Admin (fraud, abuse, etc.)
        Suspended = 4,

        // Account was deleted by the user or an admin (soft delete)
        Deleted = 5

    }
}
