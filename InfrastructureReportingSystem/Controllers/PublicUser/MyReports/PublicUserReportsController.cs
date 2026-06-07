using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MyReports;
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
    }
}
