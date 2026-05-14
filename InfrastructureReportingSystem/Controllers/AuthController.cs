using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
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
            {
                Response.Cookies.Append("refreshToken", response.RefreshToken!, new CookieOptions
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Expires = response.RefreshTokenExpiresOn
                });
                response.RefreshToken = null;
                return Ok(response);
            }
            return Unauthorized(response);
        }

        [HttpPost("Confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            var result = await _authService.ConfirmEmailAsync(dto);

            if (result)
                return Ok(new { success = true, message = "Email confirmed successfully." });
            return BadRequest(new { success = false, message = "Invalid or expired OTP code." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody]  ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new { success = true, message = "If this email exists, you will receive a password reset code." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var response = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(
            [FromBody] ResendConfirmationDto resendConfirmationDto)
        {
            var result = await _authService.ResendConfirmationEmailAsync(resendConfirmationDto.Email);

            if (result)
                return Ok(new { success = true, message = "Verification code sent successfully." });
            return BadRequest(new { success = false, message = "Invalid email or the account is already confirmed." });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { success = true, message = "Logged out successfully." });
        }

    }
}
