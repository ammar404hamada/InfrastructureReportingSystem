using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace InfraReportingSystem.Services.Auth {

    public class AuthService : IAuthService
    {
        private const string PublicUserRole = "PublicUser";
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IOtpService _otpService;

        public AuthService(
            UserManager<User> userManager, 
            IConfiguration configuration,
            IOtpService otpService
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _otpService = otpService;
        }
        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByIdAsync(confirmEmailDto.UserId);
            if (user == null) return false;

            var isValid = await _otpService.VerifyOtp(
                user.Id,
                OtpPurpose.EmailConfirmation,
                confirmEmailDto.OtpCode
                );

            if (!isValid) return false;

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            if (user.EmailConfirmed) return false;

            await _otpService.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation);

            return true;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null) return;

            await _otpService.GenerateOtp(user.Id, OtpPurpose.PasswordReset);
                
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "User does not exist."
                };

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid password."
                };

            if (!user.EmailConfirmed || user.Status == UserStatus.Inactive)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please confirm your email first."
                };
            if (user.Status == UserStatus.Suspended)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended."
                };
            if (user.Status == UserStatus.Locked)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account is locked."
                };

            var userRole = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, userRole);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful.",
                    User = new UserDto
                    {
                        Name = user.Name,
                        Email = user.Email!,
                        Role = userRole.FirstOrDefault() ?? PublicUserRole
                    },
                    Token = token
                };
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Email already exists."
                };

            var user = new User
            {
                Name = registerDto.Name,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Status = UserStatus.Inactive,
                PhoneNumber = registerDto.Phone,
                CreatedAt = DateTime.UtcNow,

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            var roleResult = await _userManager.AddToRoleAsync(user, PublicUserRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            try
            {
                await _otpService.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Registration failed because the verification code could not be sent. Please try again later."
                };
            }

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Registration successful. Please confirm your email.",
                User = new UserDto
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    Role = PublicUserRole
                }
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return false;

            var isValidOtp = await _otpService.VerifyOtp(
                user.Id,
                OtpPurpose.PasswordReset,
                resetPasswordDto.OtpCode
                );

            if (!isValidOtp) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

            if (!result.Succeeded) return false;

            if (user.Status == UserStatus.Inactive)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Worker") || roles.Contains("Authority"))
                {
                    user.Status = UserStatus.Active;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded) return false;
                }
            }

            return true;
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Name)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                    double.Parse(_configuration["JwtSettings:DurationInDays"]!)),
                signingCredentials: credentials
                );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
