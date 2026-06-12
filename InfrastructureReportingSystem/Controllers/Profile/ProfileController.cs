using System.Security.Claims;
using System.Threading.Tasks;
using InfraReportingSystem.ServiceAbstractions.Profile;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ViewProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.Profile
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Retrieves the currently authenticated user's profile.
        /// </summary>
        /// <response code="200">Returns the profile information of the user.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="404">The authenticated user no longer exists in the system.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _profileService.GetProfileAsync(userId);

            if (!response.Success && response.Message == "User not found.")
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Validation error or business rule violation (e.g., incorrect current password).</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _profileService.ChangePasswordAsync(userId, dto);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
