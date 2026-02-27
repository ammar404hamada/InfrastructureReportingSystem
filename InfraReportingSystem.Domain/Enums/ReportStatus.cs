using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Enums { 

    public class ReportStatus
    {
        public enum Status
        {
            Pending = 0,
            InProgress = 1,
            Resolved = 2,
            Rejected = 3
        }
    }
}
