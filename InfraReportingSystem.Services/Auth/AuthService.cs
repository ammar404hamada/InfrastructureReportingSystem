using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
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
    }
}