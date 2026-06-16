using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker.CurrentTaskScreen
{
    [ApiController]
    [Route("api/worker/current-task/actions")]
    [Authorize(Roles = "Worker")]
    public class WorkerCurrentTaskActionsController : ControllerBase
    {
        private readonly IWorkerCurrentTaskActionsService _actionsService;

        public WorkerCurrentTaskActionsController(IWorkerCurrentTaskActionsService actionsService)
        {
            _actionsService = actionsService;
        }

        /// <summary>
        /// Marks the worker's current task as fixed. Updates the report status and creates an audit log entry.
        /// </summary>
        /// <param name="dto">The details for marking the task as fixed, including optional notes.</param>
        /// <response code="200">The task was marked as fixed successfully.</response>
        /// <response code="400">No active task was found or the task could not be marked as fixed.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Worker role.</response>
        [HttpPost("mark-fixed")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkAsFixed([FromBody] MarkFixedDto dto)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(workerId))
                return Unauthorized(new { message = "Invalid worker identifier." });

            var result = await _actionsService.MarkAsFixedAsync(workerId, dto);

            
            if (result.StartsWith("No active") || result.Contains("not found"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }

        /// <summary>
        /// Marks the worker's current task as blocked due to an issue, with a required reason.
        /// </summary>
        /// <param name="dto">The details for marking the task as blocked, including the reason.</param>
        /// <response code="200">The task was marked as blocked successfully.</response>
        /// <response code="400">No active task was found or the reason was not provided.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Worker role.</response>
        [HttpPost("mark-blocked")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkAsBlocked([FromBody] MarkBlockedDto dto)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(workerId))
                return Unauthorized(new { message = "Invalid worker identifier." });

            var result = await _actionsService.MarkAsBlockedAsync(workerId, dto);

            if (result.StartsWith("No active") || result.StartsWith("Reason is required"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }
    }
}
