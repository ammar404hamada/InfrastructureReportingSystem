using System.Security.Claims;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.AlsoSuffer;
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

        /// <summary>
        /// Records that the currently authenticated public user is also affected by the specified report.
        /// </summary>
        /// <remarks>
        /// Creates an "Also Suffer" record linking the user to the report. Duplicate submissions are handled gracefully.
        /// </remarks>
        /// <param name="reportId">The ID of the report to confirm suffering from.</param>
        /// <response code="200">The "Also Suffer" confirmation was recorded successfully.</response>
        /// <response code="400">The confirmation could not be processed (e.g., duplicate or invalid state).</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the PublicUser role.</response>
        /// <response code="404">The specified report was not found.</response>
        [HttpPost("{reportId:int}/also-suffer")]
        [Tags("PublicUser")]
        [EndpointSummary("ConfirmAlsoSuffering")]
        [ProducesResponseType(typeof(AlsoSufferResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AlsoSufferResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AlsoSufferResponseDto), StatusCodes.Status404NotFound)]
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
