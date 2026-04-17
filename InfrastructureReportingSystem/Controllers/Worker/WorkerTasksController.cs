using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Worker")]
    public class WorkerTasksController : ControllerBase
    {
        private readonly IWorkerTasksService _workerTasksService;

        public WorkerTasksController(IWorkerTasksService workerTasksService)
        {
            _workerTasksService = workerTasksService;
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks([FromQuery] string? searchTerm)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(workerId))
                return Unauthorized();

            var tasks = await _workerTasksService.GetMyTasksAsync(workerId, searchTerm);
            return Ok(tasks);
        }
    }
}
