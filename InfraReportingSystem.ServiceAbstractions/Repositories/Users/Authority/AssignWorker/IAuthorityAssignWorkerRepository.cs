using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.AssignWorker
{
    public interface IAuthorityAssignWorkerRepository
    {
        Task<Report?> GetByIdAsync(int reportId);
        Task<bool> HasActiveTaskAsync(string workerId);
        Task SaveChangesAsync();
    }
}
