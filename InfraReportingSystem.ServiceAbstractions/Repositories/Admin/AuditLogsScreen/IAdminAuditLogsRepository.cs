using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Admin.AuditLogsScreen
{
    public interface IAdminAuditLogsRepository
    {
        Task<(List<AuditLog> Logs, int TotalCount, Dictionary<string, string> UserRoles)> GetAuditLogsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            string? normalizedRole,
            AuditActionType? actionType);
    }

}
