using InfraReportingSystem.ServiceAbstractions.Users.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Authority.Workers
{
    [ApiController]
    [Route("api/authority")]
    [Authorize(Roles = "Authority")]
    public class AuthorityWorkersController : ControllerBase
    {
        private readonly IAuthorityWorkersService _service;
        private readonly ILogger<AuthorityWorkersController> _logger;

        public AuthorityWorkersController(
            IAuthorityWorkersService service,
            ILogger<AuthorityWorkersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Returns a paginated list of workers available to the currently authenticated authority.
        /// </summary>
        /// <remarks>
        /// Supports optional search across name, email, and phone number, and pagination via page number and page size.
        /// </remarks>
        /// <param name="search">Optional search across name, email, and phone number.</param>
        /// <param name="pageNumber">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Items per page (1–100). Defaults to 50.</param>
        /// <response code="200">Returns the paginated list of workers matching the search criteria.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Authority role.</response>
        [HttpGet("workers")]
        [Tags("Authority")]
        [EndpointSummary("GetWorkersList")]
        [ProducesResponseType(typeof(PaginatedResult<WorkerListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetWorkers(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var result = await _service.GetWorkersAsync(search, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching workers list for authority.");
                return StatusCode(500, new { message = "An error occurred while retrieving workers. Please try again later." });
            }
        }
    }
}