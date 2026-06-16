using InfraReportingSystem.ServiceAbstractions.Users.Authority.IncomingReports;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.IncomingReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Authority.IncomingReports
{
    [ApiController]
    [Route("api/authority/reports")]
    [Authorize(Roles = "Authority")]
    public class AuthorityIncomingReportsController : ControllerBase
    {
        private readonly IAuthorityIncomingReportsService _service;

        public AuthorityIncomingReportsController(IAuthorityIncomingReportsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns a paginated list of incoming unassigned reports for the currently authenticated authority.
        /// </summary>
        /// <remarks>
        /// Supports optional search, sorting by field and direction, and pagination via page number and page size.
        /// </remarks>
        /// <param name="search">Optional keyword to filter reports by description or category.</param>
        /// <param name="sortBy">Optional field name to sort by (e.g., UploadedAt, Category).</param>
        /// <param name="sortDirection">Optional sort direction (asc or desc).</param>
        /// <param name="pageNumber">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Items per page (1–50). Defaults to 20.</param>
        /// <response code="200">Returns the paginated list of incoming reports matching the filters.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Authority role.</response>
        [HttpGet("incoming")]
        [Tags("Authority")]
        [EndpointSummary("GetIncomingReports")]
        [ProducesResponseType(typeof(PaginatedResult<IncomingReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetIncomingReports(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 50) pageSize = 50;

            var result = await _service.GetIncomingReportsAsync(
                search, sortBy, sortDirection, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
