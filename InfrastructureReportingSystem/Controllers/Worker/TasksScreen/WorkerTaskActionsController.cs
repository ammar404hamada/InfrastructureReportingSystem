using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksScreen;
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

    /// <summary>
    /// Accepts a task assigned to the currently authenticated worker.
    /// </summary>
    /// <param name="reportId">The ID of the report (task) to accept.</param>
    /// <response code="200">The task was accepted successfully.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Worker role.</response>
    [HttpPost("{reportId}/accept")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Rejects a task assigned to the currently authenticated worker, with a required reason.
    /// </summary>
    /// <param name="reportId">The ID of the report (task) to reject.</param>
    /// <param name="dto">The rejection details including the reason.</param>
    /// <response code="200">The task was rejected successfully.</response>
    /// <response code="400">The rejection reason was not provided.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Worker role.</response>
    [HttpPost("{reportId}/reject")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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