using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
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
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailService emailService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
        }
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null) return false;

            var decodeToken = Uri.UnescapeDataString(token);


            var result = await _userManager.ConfirmEmailAsync(user, decodeToken);
            if (!result.Succeeded) return false;

            user.Status = UserStatus.Active;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?userId={user.Id}&token={encodedToken}";

            var emailBody = $@"
                <h2>Welcome to Infrastructure Reporting System!</h2>
                <p>Hi {user.Name},</p>
                <p>We received a request to reset your password. Click the link below:</p>
                <a href='{resetLink}' 
                   style='background:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>
                   Reset Password
                </a>
                <p>If you didn't forget your password, ignore this email.</p>
            ";

            await _emailService.SendEmailAsync(user.Email!, "Reset your password", emailBody);


        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "User Does Not Exsit"
                };

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalide Password"
                };

            if (user.Status == UserStatus.Inactive)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please confirm your email first"
                };
            if (user.Status == UserStatus.Suspended)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended"
                };
            if (user.Status == UserStatus.Locked)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account is locked"
                };

            var userRole = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, userRole);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login Successful",
                    User = new UserDto
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Role = userRole.FirstOrDefault() ?? "Public user"
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
                    Message = "Email Already exists"
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
            await _userManager.AddToRoleAsync(user, "Public user");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var confirmationLink = $"{_configuration["AppUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";
            
            var emailBody = $@"
                <h2>Welcome to Infrastructure Reporting System!</h2>
                <p>Hi {user.Name},</p>
                <p>Please confirm your email by clicking the link below:</p>
                <a href='{confirmationLink}' 
                   style='background:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>
                   Confirm Email
                </a>
                <p>If you didn't register, ignore this email.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, "Confirm your Email", emailBody);

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Registeration Successful, Please confirm your Email",
                User = new UserDto
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    Role = "Public User"
                }
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user == null) return false;

            var decodedToken = Uri.UnescapeDataString(resetPasswordDto.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
                return false;
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