using InfraReportingSystem.Shared.DTOs.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Admin.AuditLogsScreen
{
    public interface IAdminAuditLogsService
    {
        Task<PaginatedResult<AuditLogDto>> GetLogsAsync(AuditLogsFilterDto filter);
    }

}
