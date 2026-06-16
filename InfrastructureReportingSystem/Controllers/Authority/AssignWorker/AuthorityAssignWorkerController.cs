using System.Security.Claims;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.AssignWorker;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.AssignWorker;
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

        /// <summary>
        /// Assigns a worker to a report on behalf of the currently authenticated authority.
        /// </summary>
        /// <remarks>
        /// Links the specified worker to the report and creates an audit log entry. Both the report and the worker must exist.
        /// </remarks>
        /// <param name="reportId">The ID of the report to assign a worker to.</param>
        /// <param name="dto">The worker assignment details containing the worker ID.</param>
        /// <response code="200">The worker was assigned to the report successfully.</response>
        /// <response code="400">The assignment could not be processed (e.g., worker already assigned).</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Authority role.</response>
        /// <response code="404">The report or the worker was not found.</response>
        [HttpPost("{reportId:int}/assign-worker")]
        [Tags("Authority")]
        [EndpointSummary("AssignWorkerToReport")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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
