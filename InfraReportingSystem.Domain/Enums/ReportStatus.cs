using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums { 

    public enum ReportStatus
    {

        Submitted = 1,                  // Initial state when a Public User successfully uploads a report.

        Assigned = 2,                   // An Authority assigns the report to a Worker.

        OnHold = 3,                      // An Authority puts the report on hold (e.g., waiting on external factors).

        Blocked = 4,                    // A Worker flags the task as incomplete/inaccessible.

        PendingConfirmation = 5,        // A Worker marks the task "Fixed"; waiting for the Public User to verify.

        Resolved = 6,                  // The Public User confirms the fix, closing the issue.

        Rejected = 7                  // The Public User Rejects The Fix, Reopening the Issue.(Was Not in The user Story)

    }
}
