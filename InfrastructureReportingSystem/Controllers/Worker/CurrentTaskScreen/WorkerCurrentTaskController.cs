using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker.CurrentTaskScreen
{
    [ApiController]
    [Route("api/worker/current-task")]
    [Authorize(Roles = "Worker")]
    public class WorkerCurrentTaskController : ControllerBase
    {
        private readonly IWorkerCurrentTaskService _currentTaskService;
        private readonly ILogger<WorkerCurrentTaskController> _logger;

        public WorkerCurrentTaskController(
            IWorkerCurrentTaskService currentTaskService,
            ILogger<WorkerCurrentTaskController> logger)
        {
            _currentTaskService = currentTaskService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the current active task for the authenticated worker, including details, actions taken, and blockage info if applicable.
        /// </summary>
        /// <remarks>
        /// A worker can only have one active task at a time. Returns null if no active task is assigned.
        /// </remarks>
        /// <response code="200">Returns the current task details for the worker, or null if none exists.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Worker role.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<CurrentTaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrentTask()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(workerId))
            {
                _logger.LogWarning("Token missing NameIdentifier claim.");
                return Unauthorized(new { message = "Invalid worker identifier." });
            }

            try
            {
                var result = await _currentTaskService.GetCurrentTaskAsync(workerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current task for worker {WorkerId}", workerId);
                return StatusCode(500, new { message = "An error occurred while retrieving the current task." });
            }
        }
    }
}
