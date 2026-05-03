using InfraReportingSystem.ServiceAbstractions.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.Admin.AuditLogsScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Admin.AuditLogsScreen
{
    [ApiController]
    [Route("api/admin/audit-logs")]
    [Authorize(Roles = "Admin")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly IAdminAuditLogsService _auditLogsService;

        public AdminAuditLogsController(IAdminAuditLogsService auditLogsService)
        {
            _auditLogsService = auditLogsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogsFilterDto filter)
        {
            var result = await _auditLogsService.GetLogsAsync(filter);
            return Ok(result);
        }
    }

}
