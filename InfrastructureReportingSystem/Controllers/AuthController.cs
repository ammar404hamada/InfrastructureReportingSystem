using Azure;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InfrastructureReportingSystem.Controllers { 

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            IAuthService authService,
            IWebHostEnvironment environment
            )
        {
            _authService = authService;
            _environment = environment;
        }

        /// <summary>
        /// Registers a new public user account.
        /// </summary>
        /// <remarks>
        /// Creates an inactive PublicUser account, assigns the PublicUser role, and sends an OTP code for email confirmation.
        /// </remarks>
        /// <response code="200">Registration succeeded and the user should confirm their email.</response>
        /// <response code="400">Registration failed because the email already exists, validation failed, role assignment failed, or the confirmation code could not be sent.</response>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        /// <summary>
        /// Logs in a confirmed active user.
        /// </summary>
        /// <remarks>
        /// Validates the email and password, creates a JWT access token, stores a refresh token in an HTTP-only cookie, and returns the logged-in user data.
        /// </remarks>
        /// <response code="200">Login succeeded and an access token was returned.</response>
        /// <response code="401">Login failed because the user does not exist, the password is invalid, the email is not confirmed, or the account is inactive, suspended, or locked.</response>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            var isProduction = _environment.IsProduction();

            if (response.Success)
            {
                Response.Cookies.Append("refreshToken", response.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = response.RefreshTokenExpiresOn
                });
                response.RefreshToken = null;
                return Ok(response);
            }
            return Unauthorized(response);
        }

        /// <summary>
        /// Creates a new access token using the refresh token cookie.
        /// </summary>
        /// <remarks>
        /// Reads the current refresh token from the HTTP-only cookie, revokes it, generates a new JWT access token, and replaces the refresh token cookie.
        /// </remarks>
        /// <response code="200">A new access token and refresh token cookie were generated successfully.</response>
        /// <response code="401">The refresh token cookie is missing, invalid, expired, revoked, or not linked to a user.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var oldRefreshToken = Request.Cookies["refreshToken"];

            if (oldRefreshToken is null)
                return Unauthorized(new { message = "No refresh token found" });

            var response = await _authService.RefreshTokenAsync(oldRefreshToken);
            
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
            else
                return Unauthorized(response);
            
        }

        /// <summary>
        /// Confirms a user's email address.
        /// </summary>
        /// <remarks>
        /// Verifies the email confirmation OTP code, marks the email as confirmed, and activates the user account.
        /// </remarks>
        /// <response code="200">The email was confirmed successfully.</response>
        /// <response code="400">The OTP code is invalid, expired, or the user email was not found.</response>
        [HttpPost("Confirm-email")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            var result = await _authService.ConfirmEmailAsync(dto);

            if (result)
                return Ok(new { success = true, message = "Email confirmed successfully." });
            return BadRequest(new { success = false, message = "Invalid or expired OTP code." });
        }

        /// <summary>
        /// Sends a password reset OTP code.
        /// </summary>
        /// <remarks>
        /// If the email belongs to an existing user, generates a password reset OTP and sends it by email. The response is always successful to avoid revealing whether the email exists.
        /// </remarks>
        /// <response code="200">The request was accepted. If the email exists, a reset code will be sent.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword(
            [FromBody]  ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new { success = true, message = "If this email exists, you will receive a password reset code." });
        }

        /// <summary>
        /// Resets a user's password using an OTP code.
        /// </summary>
        /// <remarks>
        /// Verifies the password reset OTP, resets the password, and activates inactive Worker or Authority accounts after a successful reset.
        /// </remarks>
        /// <response code="200">The password was reset successfully.</response>
        /// <response code="400">The user was not found, the OTP is invalid or expired, the new password is invalid, or account activation failed.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var response = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Resends the email confirmation OTP code.
        /// </summary>
        /// <remarks>
        /// Generates and emails a new confirmation OTP for an existing user whose email is not already confirmed.
        /// </remarks>
        /// <response code="200">A new verification code was sent successfully.</response>
        /// <response code="400">The email is invalid, the user does not exist, or the account is already confirmed.</response>
        [HttpPost("resend-confirmation")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendConfirmation(
            [FromBody] ResendConfirmationDto resendConfirmationDto)
        {
            var result = await _authService.ResendConfirmationEmailAsync(resendConfirmationDto.Email);

            if (result)
                return Ok(new { success = true, message = "Verification code sent successfully." });
            return BadRequest(new { success = false, message = "Invalid email or the account is already confirmed." });
        }

        /// <summary>
        /// Logs out the current authenticated user and close all of his sessions.
        /// </summary>
        /// <remarks>
        /// Confirms logout for an authenticated request. The client should remove its access token after this response.
        /// </remarks>
        /// <response code="200">Logout succeeded.</response>
        /// <response code="401">The request does not contain a valid JWT access token.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            await _authService.LogoutAllAsync(userId);
            return Ok(new { success = true, message = "Logged out successfully." });
        }

    }
}
