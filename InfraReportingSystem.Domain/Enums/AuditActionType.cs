using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums
{
    public enum AuditActionType
    {

        // Authentication actions
        Login = 1,
        LoginFailed = 2,
        Logout = 3,
        PasswordReset = 4,
        PasswordChanged = 23,

        // Account actions
        AccountCreated = 5,
        AccountDeleted = 6,
        AccountSuspended = 7,
        AccountReactivated = 8,
        AccountActivated = 18,
        AccountUnlocked = 19,
        AccountLocked = 20,
        AccountUpdated = 21,
        AccountVerified = 22,
        AccountDeactivated = 24,

        // Report actions
        ReportCreated = 9,
        ReportUpdated = 10,
        ReportAssigned = 11,
        ReportBlocked = 12,

        // PublicUser actions
        ReportResolved = 13,
        ReportRejected = 14,

        // Worker actions
        WorkerAcceptedTask = 15,
        WorkerRejectedTask = 16,
        WorkerMarkedTaskAsFixed = 17

    }
}
