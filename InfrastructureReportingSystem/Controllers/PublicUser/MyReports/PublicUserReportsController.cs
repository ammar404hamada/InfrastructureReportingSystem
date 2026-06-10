using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
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

        /// <param name="userId">The ID of the public user whose reports are being requested.</param>
        [HttpGet]
        [Tags("PublicUser")]
        [EndpointSummary("GetReportsByUserId")]
        public async Task<IActionResult> GetReportsByUserId([FromRoute] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required." });

            var reports = await _service.GetReportsByUserIdAsync(userId);
            return Ok(reports);
        }

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

        [HttpPost("{reportId:int}/reject-fix")]
        [Tags("PublicUser")]
        [EndpointSummary("RejectFix")]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FixConfirmationResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectFix([FromRoute] string userId, [FromRoute] int reportId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new FixConfirmationResponseDto { Success = false, Message = "userId is required." });

            var result = await _service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.FixRejected);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
