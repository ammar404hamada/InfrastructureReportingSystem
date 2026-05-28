using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
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

        [HttpGet]
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
