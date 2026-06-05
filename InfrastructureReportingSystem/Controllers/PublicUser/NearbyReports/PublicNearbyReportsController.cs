using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.NearbyReports;
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

        /// <param name="latitude">User's current latitude (-90 to 90).</param>
        /// <param name="longitude">User's current longitude (-180 to 180).</param>
        /// <param name="radiusInKm">Search radius in KM (default 3, max 50).</param>
        [HttpGet]
        [Tags("PublicUser")]
        [EndpointSummary("GetNearbyReports")]
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
