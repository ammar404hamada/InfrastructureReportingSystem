using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Admin.UserCreationScreen;
using InfraReportingSystem.Shared.DTOs.Admin.UserCreationScreen;
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

        [HttpPost]
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
