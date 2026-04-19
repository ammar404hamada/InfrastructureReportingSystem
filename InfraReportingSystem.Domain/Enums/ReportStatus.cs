using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums
{
    public enum ReportStatus
    {
        Submitted = 1,                  // Public user created report

        Assigned = 2,                   // Authority assigned it to worker

        InProgress = 3,                 // Worker accepted and is currently working on it

        OnHold = 4,                     // Put on hold by authority

        Blocked = 5,                    // Worker cannot proceed

        PendingConfirmation = 6,        // Worker marked as fixed, waiting for user confirmation

        WorkerRejected = 8,            // Worker rejected the task

        FixRejectedByUser = 9          // User rejected the fix
    }
}
