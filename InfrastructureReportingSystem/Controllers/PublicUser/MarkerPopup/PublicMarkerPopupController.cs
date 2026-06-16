using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MarkerPopup;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.MarkerPopup
{
    [ApiController]
    [Route("api/public/map-popup")]
    public class PublicMarkerPopupController : ControllerBase
    {
        private readonly IPublicMarkerPopupService _popupService;

        public PublicMarkerPopupController(IPublicMarkerPopupService popupService)
        {
            _popupService = popupService;
        }

        /// <summary>
        /// Returns the marker popup details for a publicly visible report by its ID.
        /// </summary>
        /// <remarks>
        /// Provides a summary of the report including title, description, category, status, photos, reporter info, and assigned worker (if any). This endpoint does not require authentication.
        /// </remarks>
        /// <param name="reportId">The ID of the report to retrieve.</param>
        /// <response code="200">Returns the popup details for the requested report.</response>
        /// <response code="404">The report was not found or is not publicly visible.</response>
        [HttpGet("{reportId:int}")]
        [Tags("PublicUser")]
        [EndpointSummary("GetReportMapPopup")]
        [ProducesResponseType(typeof(MarkerPopupDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMarkerPopup(int reportId)
        {
            var details = await _popupService.GetReportDetailsAsync(reportId);

            if (details is null)
                return NotFound(new { message = "Report not found or not publicly visible." });

            return Ok(details);
        }
    }
}
