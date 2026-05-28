using InfraReportingSystem.Domain.Entities;
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

        [HttpPost("mark-fixed")]
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

        [HttpPost("mark-blocked")]
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
