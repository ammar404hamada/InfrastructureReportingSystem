using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.NearbyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.NearbyReports;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.NearbyReports
{
    [ApiController]
    [Route("api/public/nearby-reports")]
    public class PublicNearbyReportsController : ControllerBase
    {
        private readonly IPublicNearbyReportsService _nearbyReportsService;

        public PublicNearbyReportsController(IPublicNearbyReportsService nearbyReportsService)
        {
            _nearbyReportsService = nearbyReportsService;
        }

        /// <summary>
        /// Returns a list of publicly visible reports near a given geographic coordinate within a specified radius.
        /// </summary>
        /// <remarks>
        /// The search radius defaults to 3 KM and can be set to a maximum of 50 KM. Latitude must be between -90 and 90, longitude between -180 and 180.
        /// </remarks>
        /// <param name="latitude">User's current latitude (-90 to 90).</param>
        /// <param name="longitude">User's current longitude (-180 to 180).</param>
        /// <param name="radiusInKm">Search radius in KM (default 3, max 50).</param>
        /// <response code="200">Returns the list of nearby reports within the specified radius.</response>
        /// <response code="400">Latitude, longitude, or radius is outside the allowed range.</response>
        [HttpGet]
        [Tags("PublicUser")]
        [EndpointSummary("GetNearbyReports")]
        [ProducesResponseType(typeof(IEnumerable<NearbyReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNearbyReports(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusInKm = 3)
        {
            if (latitude < -90 || latitude > 90)
                return BadRequest(new { message = "Latitude must be between -90 and 90." });

            if (longitude < -180 || longitude > 180)
                return BadRequest(new { message = "Longitude must be between -180 and 180." });

            if (radiusInKm <= 0)
                return BadRequest(new { message = "Radius must be greater than 0." });

            var results = await _nearbyReportsService.GetNearbyReportsAsync(
                latitude, longitude, radiusInKm);

            return Ok(results);
        }
    }

}
