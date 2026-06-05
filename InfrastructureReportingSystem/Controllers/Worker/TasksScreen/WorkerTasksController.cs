using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker.TasksScreen;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Worker")]
public class WorkerTasksController : ControllerBase
{
    private readonly IWorkerTasksService _workerTasksService;
    private readonly ILogger<WorkerTasksController> _logger;

    public WorkerTasksController(
        IWorkerTasksService workerTasksService,
        ILogger<WorkerTasksController> logger)
    {
        _workerTasksService = workerTasksService;
        _logger = logger;
    }

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] string? searchTerm,
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
            var result = await _workerTasksService.GetMyTasksAsync(workerId, searchTerm, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tasks for worker {WorkerId}", workerId);
            return StatusCode(500, new { message = "An error occurred while retrieving your tasks. Please try again later." });
        }
    }
}