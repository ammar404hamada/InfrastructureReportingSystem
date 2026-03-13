using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
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

        public AuthService(
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
                return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) 
                return false;

            user.Status = UserStatus.Active;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return new LoginResponseDto
                {
                    Message = "User Does Not Exsit"
                };

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return new LoginResponseDto
                {
                    Message = "Invalide Password"
                };

            if (user.Status == UserStatus.Inactive)
                return new LoginResponseDto
                {
                    Message = "Please confirm your email first"
                };
            if (user.Status == UserStatus.Suspended)
                return new LoginResponseDto
                {
                    Message = "Your account has been suspended"
                };
            if (user.Status == UserStatus.Locked)
                return new LoginResponseDto
                {
                    Message = "Your account is locked"
                };

            var userRole = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, userRole);

                return new LoginResponseDto
                {
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
                    Message = "Email Already exists"
                };

            var user = new User
            {
                Name = registerDto.Name,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Status = UserStatus.Inactive,
                PhoneNumber = registerDto.Phone,

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new RegisterResponseDto
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            await _userManager.AddToRoleAsync(user, "Public user");

            // TODO: send confirmation email with this token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return new RegisterResponseDto
            {
                Message = "Registeration Successful, Please confirm your Email",
                User = new UserDto
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    Role = "Public User"
                }
            };
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