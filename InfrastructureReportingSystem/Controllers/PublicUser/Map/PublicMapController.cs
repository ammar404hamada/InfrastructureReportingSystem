using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.Map;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.Map;
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

        /// <summary>
        /// Returns a list of report markers for the public map, optionally filtered by category and/or status.
        /// </summary>
        /// <param name="categoryId">Optional category ID to filter by.</param>
        /// <param name="status">Optional status string to filter by (case-insensitive, e.g., Submitted, InProgress, Resolved).</param>
        /// <response code="200">Returns the list of map markers matching the filters.</response>
        /// <response code="400">The provided status value is not a valid ReportStatus.</response>
        [HttpGet]
        [Tags("PublicUser")]
        [EndpointSummary("GetPublicMapReports")]
        [ProducesResponseType(typeof(IEnumerable<MapReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
