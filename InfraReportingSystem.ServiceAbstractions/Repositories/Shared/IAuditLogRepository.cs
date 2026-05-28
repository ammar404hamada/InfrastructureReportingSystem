using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Shared
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
    }
}
