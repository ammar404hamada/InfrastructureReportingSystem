using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.MyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.MyReports
{
    [ApiController]
    [Route("api/public/users/{userId}/reports")]
    public class PublicUserReportsController : ControllerBase
    {
        private readonly IPublicUserReportsService _service;

        public PublicUserReportsController(IPublicUserReportsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns a list of reports submitted by a specific public user, with optional filtering, sorting, and pagination.
        /// </summary>
        /// <param name="userId">The ID of the public user whose reports are being requested.</param>
        /// <param name="filter">Optional filter, sort, and pagination parameters.</param>
        /// <response code="200">Returns the list of reports for the specified user.</response>
        [HttpGet]
        [Tags("PublicUser")]
        [EndpointSummary("GetReportsByUserId")]
        [ProducesResponseType(typeof(PaginatedResult<PublicUserReportsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportsByUserId(
        [FromRoute] string userId,
        [FromQuery] PublicUserReportsFilterDto filter)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required." });

            var reports = await _service.GetReportsByUserIdAsync(userId, filter);
            return Ok(reports);
        }

        /// <summary>
        /// Confirms that a worker's fix for the specified report has been verified and accepted by the public user.
        /// </summary>
        /// <param name="userId">The ID of the public user confirming the fix.</param>
        /// <param name="reportId">The ID of the report whose fix is being confirmed.</param>
        /// <response code="200">The fix was confirmed successfully.</response>
        /// <response code="400">The confirmation could not be processed.</response>
        [HttpPost("{reportId:int}/confirm-fix")]
        [Tags("PublicUser")]
        [EndpointSummary("ConfirmFix")]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmFix([FromRoute] string userId, [FromRoute] int reportId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new FixConfirmationResponseDto { Success = false, Message = "userId is required." });

            var result = await _service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.Resolved);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Rejects a worker's fix for the specified report, indicating the issue is not yet resolved.
        /// </summary>
        /// <param name="userId">The ID of the public user rejecting the fix.</param>
        /// <param name="reportId">The ID of the report whose fix is being rejected.</param>
        /// <param name="dto">Optional reason for the rejection.</param>
        /// <response code="200">The fix was rejected successfully.</response>
        /// <response code="400">The rejection could not be processed.</response>
        [HttpPost("{reportId:int}/reject-fix")]
        [Tags("PublicUser")]
        [EndpointSummary("RejectFix")]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectFix(
            [FromRoute] string userId,
            [FromRoute] int reportId,
            [FromBody] FixRejectRequestDto? dto = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new FixConfirmationResponseDto { Success = false, Message = "userId is required." });

            var result = await _service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.FixRejected,
                dto?.Reason);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
