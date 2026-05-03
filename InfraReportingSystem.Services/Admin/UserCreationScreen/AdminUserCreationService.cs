using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Admin.UserCreationScreen;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using InfraReportingSystem.Shared.DTOs.Admin.UserCreationScreen;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Admin.UserCreationScreen
{
    public class AdminUserCreationService : IAdminUserCreationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IAuditLogRepository _auditLogRepository;

        private static readonly Regex PhoneRegex = new(@"^(010|011)\d{8}$");

        public AdminUserCreationService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IEmailService emailService,
            IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, string adminUserId)
        {

            if (string.IsNullOrWhiteSpace(request.FullName))
                return Error("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                return Error("A valid email is required.");

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return Error("Email is already in use.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !PhoneRegex.IsMatch(request.PhoneNumber))
                return Error("Phone number must be 11 digits starting with 010 or 011.");

            var roleLower = request.Role.Trim().ToLowerInvariant();
            if (roleLower != "worker" && roleLower != "authority")
                return Error("Role must be either 'Worker' or 'Authority'.");

            if (roleLower == "worker" && string.IsNullOrWhiteSpace(request.Specialization))
                return Error("Specialization is required for Worker accounts.");

            User newUser;
            string identityRole;

            if (roleLower == "worker")
            {
                newUser = new InfraReportingSystem.Domain.Entities.Worker
                {
                    Specialization = request.Specialization!.Trim()
                };
                identityRole = "Worker";
            }
            else
            {
                newUser = new InfraReportingSystem.Domain.Entities.Authority();
                identityRole = "Authority";
            }

            newUser.Name = request.FullName.Trim();
            newUser.UserName = request.Email;
            newUser.Email = request.Email;
            newUser.PhoneNumber = request.PhoneNumber;
            newUser.Status = UserStatus.Inactive;



            var placeholderPassword = Guid.NewGuid().ToString("N")[..12] + "Aa1!";


            var createResult = await _userManager.CreateAsync(newUser, placeholderPassword);
            if (!createResult.Succeeded)
                return Error(string.Join(", ", createResult.Errors.Select(e => e.Description)));


            var roleResult = await _userManager.AddToRoleAsync(newUser, identityRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser); 
                return Error(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }


            var token = await _userManager.GeneratePasswordResetTokenAsync(newUser);
            var encodedToken = Uri.EscapeDataString(token);
            var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?userId={newUser.Id}&token={encodedToken}";


            var emailBody = $@"
            <h2>Account created by admin</h2>
            <p>Hi {newUser.Name},</p>
            <p>An administrator has created an account for you. Please set your password by clicking the link below:</p>
            <a href='{resetLink}' style='background:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>
                Set Password
            </a>
            <p>If you didn't expect this email, please ignore it.</p>
        ";
            await _emailService.SendEmailAsync(newUser.Email!, "Set your password – Infrastructure Reporting System", emailBody);


            var auditLog = new AuditLog
            {
                EntityId = newUser.Id,
                ActionType = AuditActionType.AccountCreated,
                EntityName = "User",
                Details = $"Admin created {identityRole} account for {newUser.Name} ({newUser.Email}).",
                Timestamp = DateTime.UtcNow,
                UserId = adminUserId
            };
            await _auditLogRepository.AddAsync(auditLog);

            return new CreateUserResponseDto
            {
                UserId = newUser.Id,
                Message = "Account created. The user will receive an email to set their password."
            };
        }

        private static CreateUserResponseDto Error(string message) => new() { Message = message };

        private static bool IsValidEmail(string email)
        {
            try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
            catch { return false; }
        }
    }
}
