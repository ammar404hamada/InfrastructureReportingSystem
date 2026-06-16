using InfraReportingSystem.ServiceAbstractions.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Admin.UsersManagementScreen
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUsersService _adminUsersService;

        public AdminUsersController(IAdminUsersService adminUsersService)
        {
            _adminUsersService = adminUsersService;
        }

        /// <summary>
        /// Returns a paginated list of all system users with optional filtering, sorting, and pagination.
        /// </summary>
        /// <remarks>
        /// Supports filtering by search term (name/email), role, and account status. Sorts by any user field. Restricted to Admin users.
        /// </remarks>
        /// <param name="searchTerm">Optional keyword to search across name and email.</param>
        /// <param name="role">Optional role filter (e.g., Authority, PublicUser, Worker).</param>
        /// <param name="status">Optional status filter (e.g., Active, Disabled).</param>
        /// <param name="sortBy">Optional field name to sort by (e.g., Name, Email, Role).</param>
        /// <param name="sortDirection">Optional sort direction (asc or desc).</param>
        /// <param name="pageNumber">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Items per page (1–50). Defaults to 20.</param>
        /// <response code="200">Returns the paginated list of users matching the filters.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Admin role.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<AdminUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? searchTerm,
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _adminUsersService.GetUsersAsync(
                searchTerm, role, status, sortBy, sortDirection, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
