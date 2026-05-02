using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Authority.AssignWorker
{
    public interface IAuthorityAssignWorkerService
    {
        Task<(bool Success, string Message)> AssignWorkerAsync(
            int reportId,
            string workerId,
            string authorityId);
    }
}
