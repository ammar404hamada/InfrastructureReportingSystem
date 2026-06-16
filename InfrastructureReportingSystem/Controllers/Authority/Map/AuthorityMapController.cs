using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.Map;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.Map;
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

        /// <summary>
        /// Returns a list of report markers for the authority map, optionally filtered by category and/or status.
        /// </summary>
        /// <param name="categoryId">Optional category ID to filter by.</param>
        /// <param name="status">Optional status string to filter by (case-insensitive, e.g., Submitted, InProgress, Resolved).</param>
        /// <response code="200">Returns the list of authority map markers matching the filters.</response>
        /// <response code="400">The provided status value is not a valid ReportStatus.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Authority role.</response>
        [HttpGet]
        [Tags("Authority")]
        [EndpointSummary("GetAuthorityMapReports")]
        [ProducesResponseType(typeof(IEnumerable<AuthorityMapReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
