using InfraReportingSystem.ServiceAbstractions.Users.Authority.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.MarkerPopup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Authority.MarkerPopup
{
    [ApiController]
    [Route("api/authority/map-reports")]
    [Authorize(Roles = "Authority")]
    public class AuthorityMarkerPopupController : ControllerBase
    {
        private readonly IAuthorityMarkerPopupService _popupService;

        public AuthorityMarkerPopupController(IAuthorityMarkerPopupService popupService)
        {
            _popupService = popupService;
        }

        /// <summary>
        /// Returns the marker popup details for a report on the authority map.
        /// </summary>
        /// <remarks>
        /// Provides a summary of the report including title, description, category, status, photos, reporter, and assigned worker. Only reports visible on the authority map are returned.
        /// </remarks>
        /// <param name="reportId">The ID of the report to retrieve.</param>
        /// <response code="200">Returns the popup details for the requested report.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Authority role.</response>
        /// <response code="404">The report was not found or is not visible on the authority map.</response>
        [HttpGet("{reportId:int}/popup")]
        [Tags("Authority")]
        [EndpointSummary("GetReportMapPopup")]
        [ProducesResponseType(typeof(AuthorityMarkerPopupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportPopup(int reportId)
        {
            var result = await _popupService.GetReportPopupAsync(reportId);

            if (result is null)
                return NotFound(new { message = "Report not found or not visible on the authority map." });

            return Ok(result);
        }
    }
}
