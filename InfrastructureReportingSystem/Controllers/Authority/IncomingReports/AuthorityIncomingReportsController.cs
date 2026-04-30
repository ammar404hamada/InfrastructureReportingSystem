using InfraReportingSystem.ServiceAbstractions.Authority.IncomingReports;
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

        [HttpGet("incoming")]
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
