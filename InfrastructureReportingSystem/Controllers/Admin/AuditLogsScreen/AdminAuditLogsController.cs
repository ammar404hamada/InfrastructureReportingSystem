using InfraReportingSystem.ServiceAbstractions.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.AuditLogsScreen;
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

        /// <summary>
        /// Returns a paginated list of all audit log entries across the system.
        /// </summary>
        /// <remarks>
        /// Supports filtering by date range, entity type, and action type. Restricted to Admin users.
        /// </remarks>
        /// <param name="filter">The filter, sorting, and pagination parameters.</param>
        /// <response code="200">Returns the paginated list of audit log entries.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Admin role.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogsFilterDto filter)
        {
            var result = await _auditLogsService.GetLogsAsync(filter);
            return Ok(result);
        }
    }

}
