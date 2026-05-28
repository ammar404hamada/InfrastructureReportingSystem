using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.AuditLogsScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Users.Admin.AuditLogsScreen
{
    public interface IAdminAuditLogsService
    {
        Task<PaginatedResult<AuditLogDto>> GetLogsAsync(AuditLogsFilterDto filter);
    }

}
