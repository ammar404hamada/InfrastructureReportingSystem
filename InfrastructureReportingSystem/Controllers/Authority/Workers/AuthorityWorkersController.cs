using InfraReportingSystem.ServiceAbstractions.Authority.Workers;
using InfraReportingSystem.Services.Authority.Workers;
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

        public AuthorityWorkersController(IAuthorityWorkersService service)
        {
            _service = service;
        }

       
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

            var result = await _service.GetWorkersAsync(search, pageNumber, pageSize);
            return Ok(result);
        }
    }
}
