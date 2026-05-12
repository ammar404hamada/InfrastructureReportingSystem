using System.Security.Claims;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.PublicUser.AlsoSuffer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.AlsoSuffer
{
    [ApiController]
    [Route("api/public/reports")]
    [Authorize(Roles = "PublicUser")]
    public class PublicAlsoSufferController : ControllerBase
    {
        private readonly IPublicAlsoSufferService _alsoSufferService;

        public PublicAlsoSufferController(IPublicAlsoSufferService alsoSufferService)
        {
            _alsoSufferService = alsoSufferService;
        }

        [HttpPost("{reportId:int}/also-suffer")]
        public async Task<IActionResult> ConfirmAlsoSuffer(int reportId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid user identifier." });

            var result = await _alsoSufferService.ConfirmAlsoSufferAsync(reportId, userId);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
