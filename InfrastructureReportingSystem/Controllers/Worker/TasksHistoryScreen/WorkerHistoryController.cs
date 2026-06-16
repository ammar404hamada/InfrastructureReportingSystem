using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksHistoryScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker.TasksHistoryScreen
{
    [ApiController]
    [Route("api/worker/history")]
    [Authorize(Roles = "Worker")]
    public class WorkerHistoryController : ControllerBase
    {
        private readonly IWorkerHistoryService _historyService;
        private readonly ILogger<WorkerHistoryController> _logger;

        public WorkerHistoryController(IWorkerHistoryService historyService, ILogger<WorkerHistoryController> logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        /// <summary>
        /// Returns a paginated list of historical completed tasks for the currently authenticated worker.
        /// </summary>
        /// <remarks>
        /// Supports optional filtering by search term and status, and pagination via page number and page size.
        /// </remarks>
        /// <param name="searchTerm">Optional keyword to filter tasks by description or category.</param>
        /// <param name="status">Optional status filter (e.g., Resolved, Rejected).</param>
        /// <param name="pageNumber">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Items per page (1–50). Defaults to 10.</param>
        /// <response code="200">Returns the paginated task history matching the filters.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Worker role.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<HistoryTaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string? searchTerm,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(workerId))
            {
                _logger.LogWarning("Token missing NameIdentifier claim.");
                return Unauthorized(new { message = "Invalid worker identifier." });
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            try
            {
                var result = await _historyService.GetHistoryAsync(workerId, searchTerm, status, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching history for worker {WorkerId}", workerId);
                return StatusCode(500, new { message = "An error occurred while retrieving history." });
            }
        }
    }
}
