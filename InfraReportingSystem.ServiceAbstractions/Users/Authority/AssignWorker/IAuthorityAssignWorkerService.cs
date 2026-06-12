using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Users.Authority.AssignWorker
{
    public interface IAuthorityAssignWorkerService
    {
        Task<(bool Success, string Message)> AssignWorkerAsync(
            int reportId,
            string workerId,
            string authorityId);
    }
}
