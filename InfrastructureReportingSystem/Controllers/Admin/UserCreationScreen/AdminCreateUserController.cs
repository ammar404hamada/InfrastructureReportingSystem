using InfraReportingSystem.ServiceAbstractions.Users.Admin.UserCreationScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UserCreationScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers.Admin.UserCreationScreen
{
    [ApiController]
    [Route("api/admin/UserCreation")]
    [Authorize(Roles = "Admin")]
    public class AdminCreateUserController : ControllerBase
    {
        private readonly IAdminUserCreationService _creationService;

        public AdminCreateUserController(IAdminUserCreationService creationService)
        {
            _creationService = creationService;
        }

        /// <summary>
        /// Creates a new user account for a specified role (Authority, Worker, or PublicUser).
        /// </summary>
        /// <remarks>
        /// The user is created with the details provided in the request body. The creator's admin ID is captured for audit logging. Returns the new user's ID on success.
        /// </remarks>
        /// <param name="request">The user creation details including role, name, email, and password.</param>
        /// <response code="200">The user was created successfully. Returns the new user ID.</response>
        /// <response code="400">The request was invalid or user creation failed.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="403">The authenticated user does not have the Admin role.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CreateUserResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(adminUserId))
                return Unauthorized();

            var result = await _creationService.CreateUserAsync(request, adminUserId);

            if (string.IsNullOrEmpty(result.UserId))
                return BadRequest(result);

            return Ok(result);
        }
    }
}
