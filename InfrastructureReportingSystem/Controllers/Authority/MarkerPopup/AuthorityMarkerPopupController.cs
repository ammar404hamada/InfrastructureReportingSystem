using InfraReportingSystem.ServiceAbstractions.Authority.MarkerPopup;
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

        [HttpGet("{reportId:int}/popup")]
        public async Task<IActionResult> GetReportPopup(int reportId)
        {
            var result = await _popupService.GetReportPopupAsync(reportId);

            if (result is null)
                return NotFound(new { message = "Report not found or not visible on the authority map." });

            return Ok(result);
        }
    }
}
