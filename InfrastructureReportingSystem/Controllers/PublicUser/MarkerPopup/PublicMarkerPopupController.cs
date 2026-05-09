using InfraReportingSystem.ServiceAbstractions.PublicUser.MarkerPopup;
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

        [HttpGet("{reportId:int}")]
        public async Task<IActionResult> GetMarkerPopup(int reportId)
        {
            var details = await _popupService.GetReportDetailsAsync(reportId);

            if (details is null)
                return NotFound(new { message = "Report not found or not publicly visible." });

            return Ok(details);
        }
    }
}
