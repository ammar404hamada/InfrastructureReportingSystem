using InfraReportingSystem.ServiceAbstractions.Worker;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksScreen;
using InfraReportingSystem.Shared.DTOs.Worker;
using InfraReportingSystem.Shared.DTOs.Worker.TasksScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Worker.TasksScreen;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Worker")]
public class WorkerTaskActionsController : ControllerBase
{
    private readonly IWorkerTasksService _workerTasksService;
    private readonly IWorkerTaskActionsService _workerTaskActionsService;
    private readonly ILogger<WorkerTaskActionsController> _logger;

    public WorkerTaskActionsController(
        IWorkerTasksService workerTasksService,
        IWorkerTaskActionsService workerTaskActionsService,
        ILogger<WorkerTaskActionsController> logger)
    {
        _workerTasksService = workerTasksService;
        _workerTaskActionsService = workerTaskActionsService;
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

    [HttpPost("{reportId}/accept")]
    public async Task<IActionResult> AcceptTask(int reportId)
    {
        var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(workerId))
            return Unauthorized(new { message = "Invalid worker identifier." });

        try
        {
            var result = await _workerTaskActionsService.AcceptTaskAsync(reportId, workerId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting task {ReportId} for worker {WorkerId}", reportId, workerId);
            return StatusCode(500, new { message = "An error occurred while accepting the task." });
        }
    }

    [HttpPost("{reportId}/reject")]
    public async Task<IActionResult> RejectTask(int reportId, [FromBody] RejectTaskDto dto)
    {
        var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(workerId))
            return Unauthorized(new { message = "Invalid worker identifier." });

        try
        {
            var result = await _workerTaskActionsService.RejectTaskAsync(reportId, workerId, dto);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting task {ReportId} for worker {WorkerId}", reportId, workerId);
            return StatusCode(500, new { message = "An error occurred while rejecting the task." });
        }
    }
}