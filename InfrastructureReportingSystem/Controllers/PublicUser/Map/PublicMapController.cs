using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.PublicUser.Map;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.Map
{
    [ApiController]
    [Route("api/public/map-reports")]
    public class PublicMapController : ControllerBase
    {
        private readonly IPublicMapService _mapService;

        public PublicMapController(IPublicMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet]
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
