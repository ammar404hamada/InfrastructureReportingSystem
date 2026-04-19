using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums
{
    public enum ReportStatus
    {
        Submitted = 1,

        Assigned = 2,

        OnHold = 3,

        Blocked = 4,

        PendingConfirmation = 5,

        Resolved = 6,

        Rejected = 7,

        InProgress = 8,

        WorkerRejected = 9
    }
}
