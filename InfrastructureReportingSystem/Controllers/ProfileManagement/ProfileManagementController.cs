using InfraReportingSystem.ServiceAbstractions.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InfrastructureReportingSystem.Controllers.ProfileManagement
{
    [Route("api/profile-management")]
    [ApiController]
    [Authorize]
    public class ProfileManagementController : ControllerBase
    {
        private readonly IProfileManagementService _profileManagementService;

        public ProfileManagementController(IProfileManagementService profileManagementService)
        {
            _profileManagementService = profileManagementService;
        }

        /// <summary>
        /// Updates the profile information (name and/or phone number) for the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Only supplied fields that differ from the current values are updated. If no fields have changed, the operation succeeds without writing to the database.
        /// </remarks>
        /// <response code="200">Profile information was updated successfully.</response>
        /// <response code="400">Validation failed or the update request could not be processed.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="404">The authenticated user no longer exists in the system.</response>
        [HttpPut("info")]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfileInfo([FromBody] UpdateProfileInfoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new UpdateProfileResponseDto { Success = false, Message = "Unauthorized" });
            }

            var result = await _profileManagementService.UpdateProfileInfoAsync(userId, dto);

            if (!result.Success)
            {
                if (result.Message == "User not found.")
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the profile picture for the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Accepts an image file upload, uploads it to Cloudinary under a user-specific folder, and persists the URL on the user record. Only image MIME types are accepted. Maximum file size is 5 MB.
        /// </remarks>
        /// <response code="200">Profile picture was updated successfully.</response>
        /// <response code="400">No file was uploaded, the file is not an image, or the file exceeds the 5 MB size limit.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        /// <response code="404">The authenticated user no longer exists in the system.</response>
        [HttpPut("photo")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new UpdateProfileResponseDto { Success = false, Message = "No file uploaded." });
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new UpdateProfileResponseDto { Success = false, Message = "Only image files are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new UpdateProfileResponseDto { Success = false, Message = "File size must not exceed 5MB." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new UpdateProfileResponseDto { Success = false, Message = "Unauthorized" });
            }

            using var stream = file.OpenReadStream();
            var result = await _profileManagementService.UpdateProfilePhotoAsync(userId, stream, file.FileName);

            if (!result.Success)
            {
                if (result.Message == "User not found.")
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}