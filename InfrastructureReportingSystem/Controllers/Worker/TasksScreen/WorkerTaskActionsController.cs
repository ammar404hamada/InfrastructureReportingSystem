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
    private readonly IWorkerTaskActionsService _workerTaskActionsService;
    private readonly ILogger<WorkerTaskActionsController> _logger;

    public WorkerTaskActionsController(
        IWorkerTaskActionsService workerTaskActionsService,
        ILogger<WorkerTaskActionsController> logger)
    {
        _workerTaskActionsService = workerTaskActionsService;
        _logger = logger;
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

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return BadRequest(new { message = "You cannot reject a task without providing a reason." });
        }

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