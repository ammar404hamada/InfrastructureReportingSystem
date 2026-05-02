using InfraReportingSystem.ServiceAbstractions.Authority;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers;
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
        /// Returns a paginated list of active workers for use in assignment dropdowns.
        /// </summary>
        /// <param name="search">Optional search across name, email, and phone number.</param>
        /// <param name="pageNumber">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Items per page (1–100). Defaults to 50.</param>
        [HttpGet("workers")]
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