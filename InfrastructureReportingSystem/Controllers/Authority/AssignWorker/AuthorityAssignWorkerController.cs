using System.Security.Claims;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Authority.AssignWorker;
using InfraReportingSystem.Shared.DTOs.Authority.AssignWorker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Authority.AssignWorker
{
    [ApiController]
    [Route("api/authority/reports")]
    [Authorize(Roles = "Authority")]
    public class AuthorityAssignWorkerController : ControllerBase
    {
        private readonly IAuthorityAssignWorkerService _service;

        public AuthorityAssignWorkerController(IAuthorityAssignWorkerService service)
        {
            _service = service;
        }

        [HttpPost("{reportId:int}/assign-worker")]
        public async Task<IActionResult> AssignWorker(
            int reportId,
            [FromBody] AssignWorkerDto dto)
        {
            var authorityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(authorityId))
                return Unauthorized(new { message = "Invalid authority identifier." });

            var (success, message) = await _service.AssignWorkerAsync(
                reportId, dto.WorkerId, authorityId);

            if (!success)
            {
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message });

                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}
