using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksHistoryScreen;
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

        [HttpGet]
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
