using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Authority.Map;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Authority.Map
{
    [ApiController]
    [Route("api/authority/map-reports")]
    [Authorize(Roles = "Authority")]
    public class AuthorityMapController : ControllerBase
    {
        private readonly IAuthorityMapService _mapService;

        public AuthorityMapController(IAuthorityMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet]
        [Tags("Authority")]
        [EndpointSummary("GetAuthorityMapReports")]
        public async Task<IActionResult> GetMapReports(
             [FromQuery] int? categoryId,
            [FromQuery] string? status)
        {
            ReportStatus? parsedStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var result))
                    return BadRequest(new { message = $"Invalid status value: '{status}'." });

                parsedStatus = result;
            }

            var markers = await _mapService.GetMapReportsAsync(categoryId, parsedStatus);
            return Ok(markers);
        }
    }
}
