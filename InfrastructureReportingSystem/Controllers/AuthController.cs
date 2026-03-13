using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers { 

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);

            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpGet("Confirm-email")]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string token
            )
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (result)
                return Ok("Email confirmed successfully");
            return BadRequest("Invalid or expired token");
        }

    }
}