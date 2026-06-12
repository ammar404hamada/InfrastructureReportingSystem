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

        [HttpPut("info")]
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

        [HttpPut("photo")]
        [DisableRequestSizeLimit]
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